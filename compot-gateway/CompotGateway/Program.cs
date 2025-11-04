using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using CompotGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration().ConfigureBootstrapLogger().CreateBootstrapLogger();
try
{
    Log.Information("Starting application");
    builder.AddOpenTelemetry().AddSerilog();
    builder.Services
        .AddOpenApi()
        .AddCors()
        .AddCustomJwt(builder.Configuration);
    builder.Configuration.AddOcelot();
    builder.Services.AddOcelot(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseCors();
    app.UseAuthentication();
    await app.UseOcelot();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}