using OrdersService;

var builder = WebApplication.CreateBuilder(args);
builder.SetupAppServices();

var application = builder.Build();
application.MigrateDatabase();
application.SetupAppGrpcServices();

application
    .UseHttpLogging()
    .UseOpenTelemetryPrometheusScrapingEndpoint();

application.Run();