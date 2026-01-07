using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public sealed record CreateUserCommandResponse
{
    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public UserRole Role { get; init; }
    public Guid? BranchId { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}