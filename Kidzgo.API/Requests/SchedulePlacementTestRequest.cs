using System.ComponentModel.DataAnnotations;

namespace Kidzgo.API.Requests;

public sealed class SchedulePlacementTestRequest
{
    [Required]
    public Guid LeadId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    [Required]
    public DateTime ScheduledAt { get; set; }
    public string? Room { get; set; }
    public Guid? InvigilatorUserId { get; set; }
}

