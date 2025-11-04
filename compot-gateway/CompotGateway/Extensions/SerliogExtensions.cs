using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

namespace CompotGateway.Extensions;

/// <summary>
///     Extensions for configuring Serilog in the application.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    ///    Adds Serilog to the web application builder for logging.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(Log.Logger);
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .Configure()
                .AddOtel(builder)
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services);
        });
        return builder;
    }

    /// <summary>
    ///    Configures Serilog for the application.
    /// </summary>
    private static LoggerConfiguration Configure(this LoggerConfiguration loggerConfiguration)
    {
        const string logFormat = "[{Timestamp:o}] [{Level}] [T-{TraceId}] {Message}{NewLine}{Exception}";

        var config = loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .WriteTo.Console(LogEventLevel.Information, logFormat);
        return config;
    }

    private static LoggerConfiguration AddOtel(this LoggerConfiguration loggerConfiguration,
        WebApplicationBuilder builder)
    {
        var endpoint = builder.Configuration.GetValue<string>("Otel:OtlpEndpoint") ??
                       Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return loggerConfiguration;
        }

        var appName = builder.Configuration.GetValue<string>("Otel:ServiceName") ??
                      Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ??
                      System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown_service";

        return loggerConfiguration.WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = endpoint;
            options.Protocol = OtlpProtocol.Grpc;
            options.ResourceAttributes["service.name"] = appName;
            options.ResourceAttributes["service.instance.id"] = Environment.MachineName;
            options.ResourceAttributes["deployment.environment"] = builder.Environment.EnvironmentName;
        });
    }

    /// <summary>
    /// Configure minimal bootstrap logger
    /// </summary>
    public static LoggerConfiguration ConfigureBootstrapLogger(this LoggerConfiguration loggerConfiguration)
    {
        const string logFormat = "[{Timestamp:o}] [{Level}] {Message}{NewLine}{Exception}";

        return loggerConfiguration
            .MinimumLevel.Is(LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Information, logFormat);
    }
}