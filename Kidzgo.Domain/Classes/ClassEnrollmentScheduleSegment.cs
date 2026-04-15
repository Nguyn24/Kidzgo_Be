using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes;

public class ClassEnrollmentScheduleSegment : Entity
{
    public Guid Id { get; set; }
    public Guid ClassEnrollmentId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string? SessionSelectionPattern { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ClassEnrollment ClassEnrollment { get; set; } = null!;
}
