using System.Security.Cryptography;

namespace UsersService.Jwt;

public interface IJwtKeyStore
{
    RSA Rsa { get; }

    string KeyId { get; }
}