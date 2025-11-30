using Microsoft.EntityFrameworkCore;
using UsersService.Data.Entities;

namespace UsersService.Data;

public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UserEntity>(e =>
            {
                e.ToTable("users");
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Email).IsUnique();
            }
        );
    }
}