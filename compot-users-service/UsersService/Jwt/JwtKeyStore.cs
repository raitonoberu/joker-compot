using System.Security.Cryptography;

namespace UsersService.Jwt;

public sealed class JwtKeyStore
{
    public RSA Rsa => RSA.Create(2048);

    public string KeyId => "auth-key-1";
}