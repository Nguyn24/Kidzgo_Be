using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Profiles.CreateProfile;

public sealed class CreateProfileCommand : ICommand<CreateProfileResponse>
{
    public Guid UserId { get; init; }
    public ProfileType ProfileType { get; init; }
    public string DisplayName { get; init; } = null!;
    public string? PinHash { get; init; }
}

