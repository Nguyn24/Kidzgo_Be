using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.UpdateCurrentUser;

public sealed class UpdateCurrentUserCommand : ICommand<UpdateCurrentUserResponse>
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public List<UpdateProfileDto>? Profiles { get; set; }
}

public sealed class UpdateProfileDto
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
}

