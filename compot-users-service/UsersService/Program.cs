using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UsersService.Data;
using UsersService.Jwt;
using UsersService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(
            42069,
            listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
                // Без TLS для dev:
                listenOptions.UseConnectionLogging();
            }
        );
    }
);

builder.Services.AddGrpc();
builder.Services.AddSingleton<JwtKeyStore>();

builder.Services.AddDbContext<AuthDbContext>(o =>
                                                 o.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

var app = builder.Build();

app.MapGrpcService<AuthGrpcService>();

app.MapGet(
    "/.well-known/jwks.json",
    (JwtKeyStore keys) =>
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(
            new RsaSecurityKey(keys.Rsa) { KeyId = keys.KeyId, }
        );

        return Results.Json(new { keys = new[] { jwk } });
    }
);

app.Run();