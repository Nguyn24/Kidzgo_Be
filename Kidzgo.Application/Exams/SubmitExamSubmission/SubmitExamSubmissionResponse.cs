using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.SubmitExamSubmission;

public sealed class SubmitExamSubmissionResponse
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? AutoSubmittedAt { get; init; }
    public ExamSubmissionStatus Status { get; init; }
    public decimal? AutoScore { get; init; }
}


