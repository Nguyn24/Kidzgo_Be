using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.UpdateProfile;

public sealed class UpdateProfileCommand : ICommand<UpdateProfileResponse>
{
    public Guid Id { get; init; }
    public string? DisplayName { get; init; }
}

