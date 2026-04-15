namespace Kidzgo.Application.PauseEnrollmentRequests.ReassignEquivalentClass;

public sealed class ReassignEquivalentClassResponse
{
    public Guid PauseEnrollmentRequestId { get; init; }
    public Guid RegistrationId { get; init; }
    public Guid OldClassId { get; init; }
    public string OldClassName { get; init; } = null!;
    public Guid NewClassId { get; init; }
    public string NewClassName { get; init; } = null!;
    public Guid DroppedEnrollmentId { get; init; }
    public Guid NewEnrollmentId { get; init; }
    public string Track { get; init; } = null!;
    public DateTime EffectiveDate { get; init; }
    public string RegistrationStatus { get; init; } = null!;
    public DateTime OutcomeCompletedAt { get; init; }
}
