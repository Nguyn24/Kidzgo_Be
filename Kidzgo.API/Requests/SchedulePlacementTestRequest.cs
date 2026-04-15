using System.ComponentModel.DataAnnotations;

namespace Kidzgo.API.Requests;

public sealed class SchedulePlacementTestRequest
{
    /// <summary>
    /// LeadId (optional if LeadChildId is provided)
    /// </summary>
    public Guid? LeadId { get; set; }
    /// <summary>
    /// LeadChildId (priority over LeadId if provided)
    /// </summary>
    public Guid? LeadChildId { get; set; }
    [Required]
    public DateTime ScheduledAt { get; set; }
    public int? DurationMinutes { get; set; }
    [Required]
    public Guid? RoomId { get; set; }
    [Required]
    public Guid? InvigilatorUserId { get; set; }
}

