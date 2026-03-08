namespace Kidzgo.Application.Users.Admin.ReactivateUser;

public sealed class ReactivateUserCommandResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Role { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime UpdatedAt { get; init; }
}
