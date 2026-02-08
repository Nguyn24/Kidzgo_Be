using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.CreateMonthlyReportJob;

/// <summary>
/// UC-174: Tạo Monthly Report Job
/// </summary>
public sealed class CreateMonthlyReportJobCommand : ICommand<CreateMonthlyReportJobResponse>
{
    public int Month { get; init; }
    public int Year { get; init; }
    public Guid BranchId { get; init; }
}

