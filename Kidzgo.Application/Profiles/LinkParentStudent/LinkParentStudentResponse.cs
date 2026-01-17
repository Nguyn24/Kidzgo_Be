namespace Kidzgo.Application.Profiles.LinkParentStudent;

public sealed class LinkParentStudentResponse
{
    public Guid Id { get; init; }
    public Guid ParentProfileId { get; init; }
    public Guid StudentProfileId { get; init; }
    public DateTime CreatedAt { get; init; }
}

