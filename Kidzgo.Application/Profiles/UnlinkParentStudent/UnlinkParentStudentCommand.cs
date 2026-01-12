using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.UnlinkParentStudent;

public sealed class UnlinkParentStudentCommand : ICommand
{
    public Guid ParentProfileId { get; init; }
    public Guid StudentProfileId { get; init; }
}

