using Kidzgo.Domain.Users;

namespace Kidzgo.API.Requests;

public sealed class UpdateUserRequest
{
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
}

