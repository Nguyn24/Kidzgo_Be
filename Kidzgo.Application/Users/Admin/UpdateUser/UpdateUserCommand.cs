using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.Admin.UpdateUser;

public sealed class UpdateUserCommand : ICommand<UpdateUserResponse>
{
    public Guid UserId { get; set; } 
    public string? Username { get; set; }
    public string? FullName { get; set; } 
    public string? Email { get; set; } 
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? isDeleted { get; set; }
}


