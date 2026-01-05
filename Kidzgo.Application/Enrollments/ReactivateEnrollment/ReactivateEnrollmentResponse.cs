using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Enrollments.ReactivateEnrollment;

public sealed class ReactivateEnrollmentResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public DateOnly EnrollDate { get; init; }
    public EnrollmentStatus Status { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? TuitionPlanName { get; init; }
}

