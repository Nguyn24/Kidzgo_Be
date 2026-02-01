namespace Kidzgo.Application.Users.GetParentOverview;

public sealed class ParentOverviewResponse
{
    public DashboardStatistics Statistics { get; set; } = new();
    public List<StudentProfileDto> StudentProfiles { get; set; } = new();
    public List<ClassSummaryDto> Classes { get; set; } = new();
    public List<SessionSummaryDto> UpcomingSessions { get; set; } = new();
    public List<AttendanceSummaryDto> RecentAttendances { get; set; } = new();
    public List<MakeupCreditSummaryDto> MakeupCredits { get; set; } = new();
    public List<HomeworkSummaryDto> PendingHomeworks { get; set; } = new();
    public List<ExamSummaryDto> RecentExams { get; set; } = new();
    public List<ReportSummaryDto> Reports { get; set; } = new();
    public List<InvoiceSummaryDto> PendingInvoices { get; set; } = new();
    public List<MissionSummaryDto> ActiveMissions { get; set; } = new();
    public List<TicketSummaryDto> OpenTickets { get; set; } = new();
}

public sealed class DashboardStatistics
{
    public int TotalStudents { get; set; }
    public int TotalClasses { get; set; }
    public int UpcomingSessions { get; set; }
    public int AvailableMakeupCredits { get; set; }
    public int PendingHomeworks { get; set; }
    public int PendingInvoices { get; set; }
    public int ActiveMissions { get; set; }
    public int TotalStars { get; set; }
}

public sealed class StudentProfileDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public int? Level { get; set; }
    public int TotalStars { get; set; }
}

public sealed class ClassSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public Guid StudentProfileId { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class SessionSummaryDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public Guid StudentProfileId { get; set; }
    public DateTime PlannedDatetime { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class AttendanceSummaryDto
{
    public Guid SessionId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateTime SessionDate { get; set; }
    public string AttendanceStatus { get; set; } = null!;
}

public sealed class MakeupCreditSummaryDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public Guid? UsedSessionId { get; set; }
}

public sealed class HomeworkSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public Guid StudentProfileId { get; set; }
    public DateTime? DueDate { get; set; }
    public string SubmissionStatus { get; set; } = null!;
}

public sealed class ExamSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public Guid StudentProfileId { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal? Score { get; set; }
}

public sealed class ReportSummaryDto
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateTime ReportMonth { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public Guid StudentProfileId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = null!;
    public DateTime? DueDate { get; set; }
}

public sealed class MissionSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid StudentProfileId { get; set; }
    public string Status { get; set; } = null!;
    public int StarReward { get; set; }
}

public sealed class TicketSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

