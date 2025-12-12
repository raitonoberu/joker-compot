using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using ProductsService.Data;
using ProductsService.Grpc;
using ProductsService.Repositories;
using ProductsService.Repositories.Impl;
using Serilog;
using Serilog.Events;

namespace ProductsService;

public static class Setup
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsContext>();
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
            .AddScoped<IProductsRepository, ProductsRepository>()
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddDbContext<ProductsContext>(b => b.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
    }

    public static WebApplication SetupAppGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<ProductsGrpcService>();
        app.MapGrpcService<CategoriesGrpcService>();

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