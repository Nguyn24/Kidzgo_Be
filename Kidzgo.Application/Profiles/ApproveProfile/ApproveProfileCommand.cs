using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.ApproveProfile;

public sealed class ApproveProfileCommand : ICommand
{
    public Guid Id { get; init; }
}
