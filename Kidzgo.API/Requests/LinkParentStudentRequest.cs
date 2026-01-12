namespace Kidzgo.API.Requests;

public sealed class LinkParentStudentRequest
{
    public Guid ParentProfileId { get; set; }
    public Guid StudentProfileId { get; set; }
}

