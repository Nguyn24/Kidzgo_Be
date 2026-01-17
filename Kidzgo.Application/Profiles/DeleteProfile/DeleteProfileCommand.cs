using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.DeleteProfile;

public sealed class DeleteProfileCommand : ICommand
{
    public Guid Id { get; init; }
}

