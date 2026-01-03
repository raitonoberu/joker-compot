using System.Security.Cryptography;

namespace UsersService.Jwt;

public sealed class JwtKeyStore : IJwtKeyStore
{
    public RSA Rsa { get; } = RSA.Create(2048);

    public string KeyId => "auth-key-1";
}