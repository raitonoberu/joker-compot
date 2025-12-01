using UsersService;

var builder = WebApplication.CreateBuilder(args);

builder
    .SetupAppServices();

var app = builder.Build();

app
    .MigrateDatabase();

app
    .SetupAppGrpcServices()
    .SetupAppEndpoints();

app
    .UseHttpLogging()
    .UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();