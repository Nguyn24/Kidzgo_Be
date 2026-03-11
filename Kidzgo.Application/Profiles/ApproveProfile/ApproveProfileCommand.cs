using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.ApproveProfile;

public sealed class ApproveProfileCommand : ICommand<ApproveProfileResponse>
{
    public List<Guid>? Id { get; init; }
}
