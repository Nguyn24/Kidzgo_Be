using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Dashboard;

public sealed class GetDashboardQuery : IQuery<DashboardResponse>
{
    public Guid? BranchId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public sealed class DashboardResponse
{
    // Academic Dashboard
    public AttendanceStats Attendance { get; init; } = new();
    public HomeworkStats Homework { get; init; } = new();
    public LeaveStats Leave { get; init; } = new();
    public MakeupCreditStats MakeupCredits { get; init; } = new();

    // Student & Enrollment Stats
    public StudentStats Students { get; init; } = new();
    public EnrollmentStats Enrollments { get; init; } = new();

    // Lead & CRM Stats
    public LeadStats Leads { get; init; } = new();
    public PlacementTestStats PlacementTests { get; init; } = new();

    // Financial Dashboard
    public FinanceStats Finance { get; init; } = new();

    // HR Dashboard
    public HrStats HumanResources { get; init; } = new();
}

public sealed class AttendanceStats
{
    public int TotalSessions { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int LateCount { get; init; }
    public double AttendanceRate { get; init; }
}

public sealed class HomeworkStats
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Submitted { get; init; }
    public int Graded { get; init; }
    public int Overdue { get; init; }
}

public sealed class LeaveStats
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
}

public sealed class MakeupCreditStats
{
    public int TotalCredits { get; init; }
    public int UsedCredits { get; init; }
    public int AvailableCredits { get; init; }
}

public sealed class StudentStats
{
    public int Total { get; init; }
    public int Active { get; init; }
    public int Inactive { get; init; }
    public int NewThisMonth { get; init; }
}

public sealed class EnrollmentStats
{
    public int Total { get; init; }
    public int Active { get; init; }
    public int Paused { get; init; }
    public int Dropped { get; init; }
    public int NewThisMonth { get; init; }
}

public sealed class LeadStats
{
    public int Total { get; init; }
    public int New { get; init; }
    public int Contacted { get; init; }
    public int Qualified { get; init; }
    public int Enrolled { get; init; }
    public int NoShow { get; init; }
    public int Lost { get; init; }
    public double ConversionRate { get; init; }
    public int TotalTouchCount { get; init; }
}

public sealed class PlacementTestStats
{
    public int Total { get; init; }
    public int Scheduled { get; init; }
    public int Completed { get; init; }
    public int NoShow { get; init; }
    public int Enrolled { get; init; }
}

public sealed class FinanceStats
{
    public decimal TotalRevenue { get; init; }
    public decimal MonthRevenue { get; init; }
    public decimal PayOSRevenue { get; init; }
    public decimal CashRevenue { get; init; }
    public decimal OutstandingDebt { get; init; }
    public int PaidInvoices { get; init; }
    public int PendingInvoices { get; init; }
    public int OverdueInvoices { get; init; }
}

public sealed class HrStats
{
    public int TotalStaff { get; init; }
    public double TotalWorkHours { get; init; }
    public decimal TotalPayroll { get; init; }
    public int PayrollProcessed { get; init; }
    public int PayrollPending { get; init; }
}

