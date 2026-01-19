namespace Kidzgo.API.Requests;

public sealed class CheckSessionConflictsRequest
{
    public Guid? SessionId { get; set; } // Null nếu check cho session mới
    public DateTime PlannedDatetime { get; set; }
    public int DurationMinutes { get; set; }
    public Guid? PlannedRoomId { get; set; }
    public Guid? PlannedTeacherId { get; set; }
    public Guid? PlannedAssistantId { get; set; }
}