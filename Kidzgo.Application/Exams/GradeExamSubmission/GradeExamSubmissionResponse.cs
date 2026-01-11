using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.GradeExamSubmission;

public sealed class GradeExamSubmissionResponse
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
    public decimal? FinalScore { get; init; }
    public string? TeacherComment { get; init; }
    public Guid? GradedBy { get; init; }
    public DateTime? GradedAt { get; init; }
    public ExamSubmissionStatus Status { get; init; }
}


