using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UsersService.Data;
using UsersService.Grpc;
using UsersService.Jwt;
using UsersService.Repositories;
using UsersService.Services;

namespace UsersService;

public static class Setup
{
    public static WebApplicationBuilder SetupAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();

        builder
            .Services
            .AddScoped<IUsersRepository, UsersRepository>()
            .AddSingleton<IPasswordHasher, BcryptPasswordHasher>()
            .AddSingleton<IJwtKeyStore, JwtKeyStore>()
            .AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>()
            .AddDbContext<UsersDbContext>(o =>
                                              o.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
            );

        return builder;
    }

    public static WebApplicationBuilder SetupWebHost(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(
                    42069,
                    listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                        listenOptions.UseConnectionLogging();
                    }
                );
            }
        );

        return builder;
    }

    public static WebApplication SetupAppGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<AuthGrpcService>();
        app.MapGrpcService<InfoGrpcService>();

        return app;
    }

    public static WebApplication SetupAppEndpoints(this WebApplication app)
    {
        app.MapGet(
            "/.well-known/jwks.json",
            (IJwtKeyStore keys) =>
            {
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(
                    new RsaSecurityKey(keys.Rsa) { KeyId = keys.KeyId, }
                );

                return Results.Json(new { keys = new[] { jwk } });
            }
        );

        return app;
    }
}