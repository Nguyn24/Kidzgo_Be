namespace Kidzgo.Application.Users.GetTeacherOverview;

public sealed class TeacherOverviewResponse
{
    public DashboardStatistics Statistics { get; set; } = new();
    public List<ClassSummaryDto> Classes { get; set; } = new();
    public List<SessionSummaryDto> UpcomingSessions { get; set; }
    public List<StudentSummaryDto> Students { get; set; } = new();
    public List<AttendanceSummaryDto> RecentAttendances { get; set; } = new();
    public List<HomeworkSummaryDto> PendingHomeworks { get; set; } = new();
    public List<ExamSummaryDto> UpcomingExams { get; set; } = new();
    public List<ReportSummaryDto> PendingReports { get; set; } = new();
    public List<TicketSummaryDto> OpenTickets { get; set; } = new();
}

public sealed class DashboardStatistics
{
    public int TotalClasses { get; set; }
    public int TotalStudents { get; set; }
    public int UpcomingSessions { get; set; }
    public int PendingHomeworks { get; set; }
    public int PendingReports { get; set; }
    public int OpenTickets { get; set; }
}

public sealed class ClassSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Role { get; set; } = null!; // MainTeacher or AssistantTeacher
    public int StudentCount { get; set; }
    public string Status { get; set; } = null!;
}

public sealed class SessionSummaryDto
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateTime PlannedDatetime { get; set; }
    public string Status { get; set; } = null!;
    public bool AttendanceMarked { get; set; }
}

public sealed class StudentSummaryDto
{
    public Guid ProfileId { get; set; }
    public string DisplayName { get; set; } = null!;
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
}

public sealed class AttendanceSummaryDto
{
    public Guid SessionId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateTime SessionDate { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
}

public sealed class HomeworkSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public int SubmittedCount { get; set; }
    public int TotalCount { get; set; }
}

public sealed class ExamSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public Guid ClassId { get; set; }
    public string ClassCode { get; set; } = null!;
    public DateTime ExamDate { get; set; }
    public string ExamType { get; set; } = null!;
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
    public DateTime CreatedAt { get; set; }
}

