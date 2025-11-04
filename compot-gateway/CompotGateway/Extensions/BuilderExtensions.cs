using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CompotGateway.Extensions;

public static class BuilderExtensions
{
    public static AuthenticationBuilder AddCustomJwt(this IServiceCollection builder, IConfiguration configuration)
    {
        var secretKey = configuration["Auth:Key"] ?? throw new InvalidOperationException("JWT secret key is not configured.");
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        return builder
            .AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["access_token"];
                        return Task.CompletedTask;
                    },
                };
                options.RequireHttpsMetadata = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Auth:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Auth:Audience"],
                    ValidateLifetime = true
                };
            });
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection builder, IConfiguration configuration, IHostEnvironment environment)
    {
        return builder.AddCors(options =>
        {
            if (environment.IsDevelopment())
                options.AddDefaultPolicy(corsPolicyBuilder =>
                {
                    corsPolicyBuilder.SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            else
                options.AddDefaultPolicy(corsPolicyBuilder =>
                {
                    corsPolicyBuilder.WithOrigins(configuration.GetSection("Domain").Get<string[]>() ?? [])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
    }
}