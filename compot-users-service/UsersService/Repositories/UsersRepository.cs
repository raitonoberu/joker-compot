using Microsoft.EntityFrameworkCore;
using UsersService.Data;
using UsersService.Data.Entities;

namespace UsersService.Repositories;

public sealed class UsersRepository(UsersDbContext dbContext) : IUsersRepository
{
    private readonly UsersDbContext _dbContext = dbContext;

    public Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken)
    {
        return _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext
               .Users
               .AsNoTracking()
               .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<UserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return _dbContext
               .Users
               .AsNoTracking()
               .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<UserEntity> CreateAsync(
        string email,
        string passwordHash,
        CancellationToken cancellationToken
    )
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return user;
    }
}