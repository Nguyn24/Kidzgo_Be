namespace Kidzgo.API.Requests;

public sealed class CreateEnrollmentRequest
{
    public Guid ClassId { get; set; }
    public Guid StudentProfileId { get; set; }
    public DateOnly EnrollDate { get; set; }
    public Guid? TuitionPlanId { get; set; }
}

