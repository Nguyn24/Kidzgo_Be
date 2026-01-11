using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.StartExamSubmission;

public sealed class StartExamSubmissionResponse
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
    public DateTime? ActualStartTime { get; init; }
    public ExamSubmissionStatus Status { get; init; }
    public DateTime? ScheduledStartTime { get; init; }
    public int? TimeLimitMinutes { get; init; }
}


