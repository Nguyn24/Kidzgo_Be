namespace Kidzgo.Application.MonthlyReports.CreateMonthlyReportJob;

public sealed class CreateMonthlyReportJobResponse
{
    public Guid Id { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public Guid BranchId { get; init; }
    public string Status { get; init; } = null!;
    public Guid? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}

