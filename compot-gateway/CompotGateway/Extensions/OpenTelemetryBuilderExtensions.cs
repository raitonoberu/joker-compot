using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CompotGateway.Extensions;

public static class OpenTelemetryBuilderExtensions
{
    public static WebApplicationBuilder AddOpenTelemetry(this WebApplicationBuilder builder)
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
                    .AddGrpcClientInstrumentation()
                    .AddOtlpExporter(otlpOptions => { otlpOptions.Endpoint = endpoint; });
            });
        return builder;
    }
}