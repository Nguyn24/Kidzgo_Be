using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.ReactivateProfile;

public sealed class ReactivateProfileCommand : ICommand
{
    public Guid Id { get; init; }
}

