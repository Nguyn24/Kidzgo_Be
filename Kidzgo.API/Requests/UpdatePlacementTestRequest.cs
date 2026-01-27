namespace Kidzgo.API.Requests;

public sealed class UpdatePlacementTestRequest
{
    public DateTime? ScheduledAt { get; set; }
    public string? Room { get; set; }
    public Guid? InvigilatorUserId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
}

