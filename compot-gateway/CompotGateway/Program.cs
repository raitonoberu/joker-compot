using Serilog;
using CompotGateway.Extensions;
using CompotGateway.Services;
using CompotGateway.Middleware;
using UsersService.Protos;
using ProductsService.Protos;
using System.Threading.RateLimiting;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration().ConfigureBootstrapLogger().CreateBootstrapLogger();
try
{
    Log.Information("Starting application");
    IConnectionMultiplexer redisConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(builder.Configuration.GetConnectionString("Redis"));
    builder.Services.AddSingleton(redisConnectionMultiplexer);
    builder.AddOpenTelemetry().AddSerilog();
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.ConnectionMultiplexerFactory = () => Task.FromResult(redisConnectionMultiplexer);
        options.InstanceName = "CompotGateway_";
    });


    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    QueueLimit = 0,
                    Window = TimeSpan.FromMinutes(1)
                });
        });
        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
        };
    });

    builder.Services.AddGrpcClient<InfoService.InfoServiceClient>(o => o.Address = new Uri(builder.Configuration["Services:Users"]))
        .AddStandardResilienceHandler();
    builder.Services.AddGrpcClient<AuthService.AuthServiceClient>(o => o.Address = new Uri(builder.Configuration["Services:Users"]))
        .AddStandardResilienceHandler();
    builder.Services.AddGrpcClient<OrdersService.Protos.OrdersService.OrdersServiceClient>(o => o.Address = new Uri(builder.Configuration["Services:Orders"]))
        .AddStandardResilienceHandler();
    builder.Services.AddGrpcClient<ProductService.ProductServiceClient>(o => o.Address = new Uri(builder.Configuration["Services:Products"]))
        .AddStandardResilienceHandler();
    builder.Services.AddGrpcClient<CategoryService.CategoryServiceClient>(o => o.Address = new Uri(builder.Configuration["Services:Products"]))
        .AddStandardResilienceHandler();

    builder.Services.AddScoped<ProfileAggregator>();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services
        .AddControllers();
    builder.Services
        .AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer(async (document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Compot API",
                    Version = "v1"
                };

                document.Components ??= new OpenApiComponents();

                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                };
                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer"),
                        []
                    }
                });
                await Task.CompletedTask;
            });
        })
        .AddCors()
        .AddCustomJwt(builder.Configuration);
    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.UseExceptionHandler();

    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}