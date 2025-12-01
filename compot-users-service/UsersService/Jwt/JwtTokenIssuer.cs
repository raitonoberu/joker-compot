using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace UsersService.Jwt;

public sealed class JwtTokenIssuer(IJwtKeyStore keyStore) : IJwtTokenIssuer
{
    private readonly IJwtKeyStore _keyStore = keyStore;

    public string Issue(Guid userId)
    {
        var key = new RsaSecurityKey(_keyStore.Rsa)
        {
            KeyId = _keyStore.KeyId,
        };

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.RsaSha256
        );

        var token = new JwtSecurityToken(
            "http://users-service",
            "internal",
            [
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            ],
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}