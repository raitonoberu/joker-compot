using UsersService;

var builder = WebApplication.CreateBuilder(args);

builder
    .SetupAppServices()
    .SetupWebHost();

var app = builder.Build();

app
    .SetupAppGrpcServices()
    .SetupAppEndpoints();

app.Run();