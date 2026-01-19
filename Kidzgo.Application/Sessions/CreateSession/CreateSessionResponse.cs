namespace Kidzgo.Application.Sessions.CreateSession;

public sealed class CreateSessionResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime PlannedDatetime { get; init; }
    public int DurationMinutes { get; init; }
}





