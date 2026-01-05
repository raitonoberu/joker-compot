using UsersService;

var builder = WebApplication.CreateBuilder(args);

builder
    .SetupAppServices();

var app = builder.Build();
app.MigrateDatabase();
app.SetupAppGrpcServices();
app.SetupAppEndpoints();
app.UseHttpLogging();

app.Run();