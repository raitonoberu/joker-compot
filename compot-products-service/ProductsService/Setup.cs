using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
        builder.AddOpenTelemetry();
        builder.Services.AddGrpc();
        builder.AddLogging();

        builder
            .Services
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

    private static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
    {
        var url = builder.Configuration.GetValue<string>("Otel:OtlpEndpoint") ??
                  Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        if (string.IsNullOrWhiteSpace(url))
        {
            return builder;
        }

        var endpoint = new Uri(url);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: builder.Configuration.GetValue<string>("Otel:ServiceName") ??
                                 Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ??
                                 System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown_service"
                    , serviceInstanceId: Environment.MachineName)
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
                ]))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(otlpOptions => { otlpOptions.Endpoint = endpoint; });
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(otlpOptions => { otlpOptions.Endpoint = endpoint; });
            });
        return builder;
    }
}