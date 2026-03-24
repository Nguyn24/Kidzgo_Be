namespace Kidzgo.API.Requests;

public sealed class RetakePlacementTestRequest
{
    public Guid StudentProfileId { get; set; }
    public Guid NewProgramId { get; set; }
    public Guid NewTuitionPlanId { get; set; }
    public Guid BranchId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? Room { get; set; }
    public Guid? InvigilatorUserId { get; set; }
    public string? Note { get; set; }
}
