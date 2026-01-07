using Kidzgo.Domain.Users;

namespace Kidzgo.API.Requests;

public sealed class CreateUserRequest
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRole Role { get; set; }
}

