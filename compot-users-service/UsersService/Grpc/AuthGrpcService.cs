using Grpc.Core;
using UsersService.Jwt;
using UsersService.Protos;
using UsersService.Repositories;
using UsersService.Services;

namespace UsersService.Grpc;

public class AuthGrpcService(IUsersRepository usersRepository, IJwtTokenIssuer tokenIssuer, IPasswordHasher passwordHasher)
    : AuthService.AuthServiceBase
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    private readonly IJwtTokenIssuer _tokenIssuer = tokenIssuer;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    public override async Task<AuthResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        if (await _usersRepository.IsEmailExistAsync(request.Email, context.CancellationToken))
            throw new RpcException(new Status(StatusCode.AlreadyExists, "email exists"));

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = await _usersRepository.CreateAsync(request.Email, passwordHash, context.CancellationToken);

        return new AuthResponse { AccessToken = _tokenIssuer.Issue(user.Id) };
    }

    public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email, context.CancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new RpcException(new Status(StatusCode.Unauthenticated, "invalid credentials"));

        return new AuthResponse { AccessToken = _tokenIssuer.Issue(user.Id) };
    }
}