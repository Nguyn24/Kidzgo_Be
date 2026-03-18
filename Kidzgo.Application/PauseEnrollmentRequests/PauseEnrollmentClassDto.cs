namespace Kidzgo.Application.PauseEnrollmentRequests;

public sealed class PauseEnrollmentClassDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public Guid ProgramId { get; set; }
    public string ProgramName { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string Status { get; set; } = null!;
}
