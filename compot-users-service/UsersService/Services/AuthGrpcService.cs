using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UsersService.Data;
using UsersService.Jwt;
using UsersService.Protos;

namespace UsersService.Services;

public class AuthGrpcService(AuthDbContext db, JwtKeyStore keys) : AuthService.AuthServiceBase
{
    private readonly AuthDbContext _db = db;
    private readonly JwtKeyStore _keys = keys;

    public override async Task<AuthResponse> Register(RegisterRequest r, ServerCallContext ctx)
    {
        if (await _db.Users.AnyAsync(x => x.Email == r.Email))
            throw new RpcException(new Status(StatusCode.AlreadyExists, "email exists"));

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = r.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password),
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new AuthResponse { AccessToken = IssueToken(user.Id) };
    }

    public override async Task<AuthResponse> Login(LoginRequest r, ServerCallContext ctx)
    {
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == r.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(r.Password, user.PasswordHash))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "invalid credentials"));

        return new AuthResponse { AccessToken = IssueToken(user.Id) };
    }

    private string IssueToken(Guid userId)
    {
        var key = new RsaSecurityKey(_keys.Rsa) { KeyId = _keys.KeyId, };
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var jwt = new JwtSecurityToken(
            issuer: "http://users.service",
            audience: "internal",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),],
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}