using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Enrollments.GetEnrollmentById;

public sealed class GetEnrollmentByIdResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public DateOnly EnrollDate { get; init; }
    public string Status { get; init; } = null!;
    public Guid? TuitionPlanId { get; init; }
    public string? TuitionPlanName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

