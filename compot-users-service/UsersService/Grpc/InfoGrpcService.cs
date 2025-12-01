using Grpc.Core;
using UsersService.Protos;
using UsersService.Repositories;

namespace UsersService.Grpc;

public sealed class InfoGrpcService(IUsersRepository usersRepository, ILogger<InfoGrpcService> logger)
    : InfoService.InfoServiceBase
{
    private readonly ILogger<InfoGrpcService> _logger = logger;
    private readonly IUsersRepository _usersRepository = usersRepository;

    public override async Task<UserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid user_id"));
        }

        var user = await _usersRepository.GetByIdAsync(userId, context.CancellationToken);

        if (user is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "user not found"));
        }

        _logger.LogInformation("User with email {email} found", user.Email);

        return new UserResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email,
        };
    }
}