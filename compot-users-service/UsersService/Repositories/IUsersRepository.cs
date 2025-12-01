using UsersService.Data.Entities;

namespace UsersService.Repositories;

public interface IUsersRepository
{
    Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken);

    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserEntity> CreateAsync(string email, string passwordHash, CancellationToken cancellationToken);
}