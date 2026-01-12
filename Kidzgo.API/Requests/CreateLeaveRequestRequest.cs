namespace Kidzgo.API.Requests;

public sealed class CreateLeaveRequestRequest
{
    public Guid StudentProfileId { get; set; }
    public Guid ClassId { get; set; }
    public DateOnly SessionDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Reason { get; set; }
}

