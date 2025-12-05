using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OrdersService.Data;
using OrdersService.Grpc;
using OrdersService.Repositories;
using Serilog;
using Serilog.Events;

namespace OrdersService;

public static class Setup
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrdersContext>();
        db.Database.Migrate();
    }

    public static void SetupAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        builder.AddLogging();   

        builder
            .Services
            .AddTelemetry()
            .AddHttpLogging()
            .AddScoped<IOrdersRepository, OrdersRepository>()
            .AddDbContext<OrdersContext>(b => b.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
    }

    public static WebApplication SetupAppGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<OrdersGrpcService>();

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