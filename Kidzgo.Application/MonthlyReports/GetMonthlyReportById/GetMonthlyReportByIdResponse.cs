namespace Kidzgo.Application.MonthlyReports.GetMonthlyReportById;

public sealed class GetMonthlyReportByIdResponse
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public Guid? ProgramId { get; init; }
    public string? ProgramName { get; init; }
    public Guid? JobId { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
    public string? DraftContent { get; init; }
    public string? FinalContent { get; init; }
    public string Status { get; init; } = null!;
    public string? PdfUrl { get; init; }
    public DateTime? PdfGeneratedAt { get; init; }
    public Guid? SubmittedBy { get; init; }
    public string? SubmittedByName { get; init; }
    public Guid? ReviewedBy { get; init; }
    public string? ReviewedByName { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public MonthlyReportDataDto? Data { get; init; }
    public List<ReportCommentDto> Comments { get; init; } = new();
}

public sealed class MonthlyReportDataDto
{
    public string? AttendanceData { get; init; }
    public string? HomeworkData { get; init; }
    public string? TestData { get; init; }
    public string? MissionData { get; init; }
    public string? NotesData { get; init; }
    public string? TopicsData { get; init; }
}

public sealed class ReportCommentDto
{
    public Guid Id { get; init; }
    public Guid CommenterId { get; init; }
    public string CommenterName { get; init; } = null!;
    public string Content { get; init; } = null!;
    public DateTime CreatedAt { get; init; }
}

