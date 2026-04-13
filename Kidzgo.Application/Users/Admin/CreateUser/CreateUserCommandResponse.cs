using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public sealed record CreateUserCommandResponse
{
    public Guid Id { get; init; }
    public string? Username { get; init; }
    public string? Name { get; init; }
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public string Role { get; init; } = null!;
    public string? TeacherCompensationType { get; init; }
    public Guid? BranchId { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
