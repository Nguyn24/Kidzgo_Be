using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.CheckSessionConflicts;

public sealed class CheckSessionConflictsQuery : IQuery<CheckSessionConflictsResponse>
{
    public Guid? SessionId { get; init; } // Null nếu check cho session mới
    public DateTime PlannedDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public Guid? PlannedRoomId { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public Guid? PlannedAssistantId { get; init; }
}