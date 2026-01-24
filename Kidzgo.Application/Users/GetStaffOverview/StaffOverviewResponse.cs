namespace Kidzgo.Application.Users.GetStaffOverview;

public sealed class StaffOverviewResponse
{
    public DashboardStatistics Statistics { get; set; } = new();
    public List<LeadSummaryDto> RecentLeads { get; set; } = new();
    public List<EnrollmentSummaryDto> RecentEnrollments { get; set; } = new();
    public List<ClassSummaryDto> Classes { get; set; } = new();
    public List<SessionSummaryDto> UpcomingSessions { get; set; } = new();
    public List<MakeupCreditSummaryDto> PendingMakeupCredits { get; set; } = new();
    public List<LeaveRequestSummaryDto> PendingLeaveRequests { get; set; } = new();
    public List<InvoiceSummaryDto> PendingInvoices { get; set; } = new();
    public List<ReportSummaryDto> PendingReports { get; set; } = new();
    public List<TicketSummaryDto> OpenTickets { get; set; } = new();
}

public sealed class DashboardStatistics
{
    public int TotalLeads { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalClasses { get; set; }
    public int UpcomingSessions { get; set; }
    public int PendingMakeupCredits { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int PendingInvoices { get; set; }
    public int PendingReports { get; set; }
    public int OpenTickets { get; set; }
}

public sealed class LeadSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public sealed class EnrollmentSummaryDto
{
    public Guid Id { get; set; }
    public string ClassCode { get; set; } = null!;
    public string StudentName { get; set; } = null!;
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class ClassSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public int EnrollmentCount { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class SessionSummaryDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateTime PlannedDatetime { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class MakeupCreditSummaryDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
}

public sealed class LeaveRequestSummaryDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = null!;
    public DateTime RequestDate { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public string StudentName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = null!;
}

public sealed class ReportSummaryDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = null!;
    public string ClassCode { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime ReportMonth { get; set; }
}

public sealed class TicketSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

