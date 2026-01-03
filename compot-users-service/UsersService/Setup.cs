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

    private static readonly string[] Data = new[] { "id_token" };
    private static readonly string[] DataArray = new[] { "public" };
    private static readonly string[] DataArray0 = new[] { "RS256" };

    public static WebApplication SetupAppEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/.well-known/jwks.json",
            (IJwtKeyStore keys) =>
            {
                var parameters = keys.Rsa.ExportParameters(false);
                var jwk = new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = keys.KeyId,
                    alg = "RS256",
                    n = Base64UrlEncoder.Encode(parameters.Modulus),
                    e = Base64UrlEncoder.Encode(parameters.Exponent)
                };

                return Results.Json(new { keys = new[] { jwk, }, });
            }
        );

        app.MapGet(
            "/.well-known/openid-configuration",
            (HttpContext ctx) =>
                Results.Json(new
                {
                    issuer = $"{ctx.Request.Scheme}://{ctx.Request.Host}",
                    jwks_uri = $"{ctx.Request.Scheme}://{ctx.Request.Host}/.well-known/jwks.json",
                    response_types_supported = Data,
                    subject_types_supported = DataArray,
                    id_token_signing_alg_values_supported = DataArray0,
                })
        );


        return app;
    }

    private static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        const string logMessageTemplate =
            "[{Timestamp:HH:mm:ss.fff}] {Level:u3} [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft",
                builder.Environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning)
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