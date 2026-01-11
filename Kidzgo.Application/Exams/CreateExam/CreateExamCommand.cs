using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.CreateExam;

public sealed class CreateExamCommand : ICommand<CreateExamResponse>
{
    public Guid ClassId { get; init; }
    public ExamType ExamType { get; init; }
    public DateOnly Date { get; init; }
    public decimal? MaxScore { get; init; }
    public string? Description { get; init; }
    
    // Thời gian thi (cho thi tại trung tâm)
    public DateTime? ScheduledStartTime { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool AllowLateStart { get; init; }
    public int? LateStartToleranceMinutes { get; init; }
    
    // Settings
    public bool AutoSubmitOnTimeLimit { get; init; }
    public bool PreventCopyPaste { get; init; }
    public bool PreventNavigation { get; init; }
    public bool ShowResultsImmediately { get; init; }
}

