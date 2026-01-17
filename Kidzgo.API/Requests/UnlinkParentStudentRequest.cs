namespace Kidzgo.API.Requests;

public sealed class UnlinkParentStudentRequest
{
    public Guid ParentProfileId { get; set; }
    public Guid StudentProfileId { get; set; }
}

