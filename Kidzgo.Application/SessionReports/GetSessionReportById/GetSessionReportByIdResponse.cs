using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.SessionReports.GetSessionReportById;

public sealed class GetSessionReportByIdResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public DateTime SessionDate { get; init; }
    public SessionStatus SessionStatus { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid TeacherUserId { get; init; }
    public string TeacherName { get; init; } = null!;
    public DateOnly ReportDate { get; init; }
    public string Feedback { get; init; } = null!;
    public string? AiGeneratedSummary { get; init; }
    public bool IsMonthlyCompiled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

