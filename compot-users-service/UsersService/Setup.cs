using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using Serilog;
using Serilog.Events;
using UsersService.Data;
using UsersService.Grpc;
using UsersService.Jwt;
using UsersService.Repositories;
using UsersService.Services;

namespace UsersService;

public static class Setup
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        db.Database.Migrate();
    }

    public static WebApplicationBuilder SetupAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.AddLogging();

        builder
            .Services
            .AddTelemetry()
            .AddHttpLogging()
            .AddScoped<IUsersRepository, UsersRepository>()
            .AddSingleton<IPasswordHasher, BcryptPasswordHasher>()
            .AddSingleton<IJwtKeyStore, JwtKeyStore>()
            .AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>()
            .AddDbContext<UsersDbContext>(b => b.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

        return builder;
    }

    public static WebApplication SetupAppGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<AuthGrpcService>();
        app.MapGrpcService<InfoGrpcService>();

        return app;
    }

    public static WebApplication SetupAppEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/.well-known/jwks.json",
            (IJwtKeyStore keys) =>
            {
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(
                    new RsaSecurityKey(keys.Rsa) { KeyId = keys.KeyId, }
                );

                return Results.Json(new { keys = new[] { jwk, }, });
            }
        );

        return app;
    }

    private static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        const string logMessageTemplate =
            "[{Timestamp:HH:mm:ss.fff}] {Level:u3} [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        var loggerConfig = new LoggerConfiguration()
                           .MinimumLevel.Information()
                           .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                           .Enrich.FromLogContext()
                           .WriteTo.Console(outputTemplate: logMessageTemplate);

        Log.Logger = loggerConfig.CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        return builder;
    }

    private static IServiceCollection AddTelemetry(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                                    .AddAspNetCoreInstrumentation()
                                    .AddPrometheusExporter()
            );

        return services;
    }
}