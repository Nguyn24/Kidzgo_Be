using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Profiles.LinkParentStudent;

public sealed class LinkParentStudentCommand : ICommand<LinkParentStudentResponse>
{
    public Guid ParentProfileId { get; init; }
    public Guid StudentProfileId { get; init; }
}

