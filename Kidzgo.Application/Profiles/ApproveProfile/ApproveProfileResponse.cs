namespace Kidzgo.Application.Profiles.ApproveProfile;

public sealed class ApproveProfileResponse
{
    public int ApprovedCount { get; set; }
    public List<Guid> AlreadyApproved { get; set; } = [];
    public List<Guid> NotFound { get; set; } = [];
}