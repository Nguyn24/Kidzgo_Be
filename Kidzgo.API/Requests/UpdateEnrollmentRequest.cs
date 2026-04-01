namespace Kidzgo.API.Requests;

public sealed class UpdateEnrollmentRequest
{
    public DateOnly? EnrollDate { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public string? Track { get; set; }
    public string? SessionSelectionPattern { get; set; }
    public bool ClearSessionSelectionPattern { get; set; }
}

