using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.GetExams;

public sealed class ExamDto
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public ExamType ExamType { get; init; }
    public DateOnly Date { get; init; }
    public decimal? MaxScore { get; init; }
    public string? Description { get; init; }
    public DateTime? ScheduledStartTime { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool AllowLateStart { get; init; }
    public int? LateStartToleranceMinutes { get; init; }
    public bool AutoSubmitOnTimeLimit { get; init; }
    public bool PreventCopyPaste { get; init; }
    public bool PreventNavigation { get; init; }
    public bool ShowResultsImmediately { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ResultCount { get; init; }
}

