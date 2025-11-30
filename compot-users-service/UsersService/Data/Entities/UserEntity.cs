using System.ComponentModel.DataAnnotations;

namespace UsersService.Data.Entities;

public sealed class UserEntity
{
    public required Guid Id { get; set; }

    [Required]
    [MaxLength(64)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(128)]
    public required string PasswordHash { get; set; }
}