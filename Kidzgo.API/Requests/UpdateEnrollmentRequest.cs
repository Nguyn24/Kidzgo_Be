namespace Kidzgo.API.Requests;

public sealed class UpdateEnrollmentRequest
{
    public DateOnly? EnrollDate { get; set; }
    public Guid? TuitionPlanId { get; set; }
}

