using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CompotGateway.Extensions;

public static class BuilderExtensions
{
    public static AuthenticationBuilder AddCustomJwt(this IServiceCollection builder, IConfiguration configuration)
    {
        var metadataUrl = configuration["Auth:OpenIdConfigUrl"];

        return builder
            .AddAuthentication()
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    metadataUrl,
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever { RequireHttps = false }
                );

                //todo: validate issuer and audience
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
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