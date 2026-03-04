using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Dashboard.GetDashboard;

public sealed class GetDashboardQuery : IQuery<DashboardResponse>
{
    public Guid? BranchId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public sealed class DashboardResponse
{
    // Academic Dashboard
    public DashboardAttendance Attendance { get; init; } = new();
    public DashboardHomework Homework { get; init; } = new();
    public DashboardLeaveRequest LeaveRequest { get; init; } = new();
    public DashboardMakeupCredit MakeupCredit { get; init; } = new();

    // Student & Enrollment
    public int ActiveStudents { get; init; }
    public int TotalStudents { get; init; }
    public int Enrollments { get; init; }
    public int PendingEnrollments { get; init; }

    // CRM Dashboard
    public DashboardLead Leads { get; init; } = new();
    public DashboardPlacementTest PlacementTests { get; init; } = new();

    // Financial Dashboard
    public DashboardFinance Finance { get; init; } = new();

    // HR Dashboard
    public DashboardHr Hr { get; init; } = new();
}

public sealed class DashboardAttendance
{
    public int TotalSessions { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int LateCount { get; init; }
    public double AttendanceRate { get; init; }
}

public sealed class DashboardHomework
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Submitted { get; init; }
    public int Graded { get; init; }
    public int Overdue { get; init; }
}

public sealed class DashboardLeaveRequest
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Approved { get; init; }
    public int Rejected { get; init; }
}

public sealed class DashboardMakeupCredit
{
    public int TotalCredits { get; init; }
    public int UsedCredits { get; init; }
    public int RemainingCredits { get; init; }
}

public sealed class DashboardLead
{
    public int Total { get; init; }
    public int NewThisMonth { get; init; }
    public int Converted { get; init; }
    public int NoShow { get; init; }
    public int TotalTouches { get; init; }
    public double ConversionRate { get; init; }
}

public sealed class DashboardPlacementTest
{
    public int Total { get; init; }
    public int Pending { get; init; }
    public int Completed { get; init; }
    public int Enrolled { get; init; }
}

public sealed class DashboardFinance
{
    public decimal TotalRevenue { get; init; }
    public decimal PayOSRevenue { get; init; }
    public decimal CashRevenue { get; init; }
    public decimal TotalOutstanding { get; init; }
    public int OutstandingInvoices { get; init; }
    public int PaidInvoices { get; init; }
}

public sealed class DashboardHr
{
    public int TotalStaff { get; init; }
    public double TotalWorkHours { get; init; }
    public decimal TotalPayroll { get; init; }
    public int ActiveContracts { get; init; }
}

