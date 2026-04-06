namespace Kidzgo.Application.Sessions.CreateSession;

public sealed class CreateSessionResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime PlannedDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public Guid SessionId => Id;
    public DateOnly PlannedDate => DateOnly.FromDateTime(PlannedDatetime);
    public TimeOnly StartTime => TimeOnly.FromDateTime(PlannedDatetime);
    public TimeOnly EndTime => TimeOnly.FromDateTime(PlannedDatetime.AddMinutes(DurationMinutes));
}


