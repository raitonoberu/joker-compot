using Grpc.Core;
using UsersService.Jwt;
using UsersService.Protos;
using UsersService.Repositories;
using UsersService.Services;

namespace UsersService.Grpc;

public class AuthGrpcService(
    IUsersRepository usersRepository,
    IJwtTokenIssuer tokenIssuer,
    IPasswordHasher passwordHasher,
    ILogger<AuthGrpcService> logger
)
    : AuthService.AuthServiceBase
{
    private readonly ILogger<AuthGrpcService> _logger = logger;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenIssuer _tokenIssuer = tokenIssuer;
    private readonly IUsersRepository _usersRepository = usersRepository;

    public override async Task<AuthResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        if (await _usersRepository.IsEmailExistAsync(request.Email, context.CancellationToken))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "email exists"));
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = await _usersRepository.CreateAsync(request.Email, passwordHash, context.CancellationToken);

        _logger.LogInformation("New user with email {email} registered", request.Email);

        return new AuthResponse { AccessToken = _tokenIssuer.Issue(user.Id), };
    }

    public override async Task<AuthResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var user = await _usersRepository.GetByEmailAsync(request.Email, context.CancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, "invalid credentials"));
        }

        _logger.LogInformation("User with email {email} logged in", request.Email);

        return new AuthResponse { AccessToken = _tokenIssuer.Issue(user.Id), };
    }
}