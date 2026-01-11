using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.GetExamSubmissions;

public sealed class GetExamSubmissionsResponse
{
    public Page<ExamSubmissionDto> Submissions { get; init; } = null!;
}

public sealed class ExamSubmissionDto
{
    public Guid Id { get; init; }
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
    public string? StudentName { get; init; }
    public DateTime? ActualStartTime { get; init; }
    public DateTime? SubmittedAt { get; init; }
    public DateTime? AutoSubmittedAt { get; init; }
    public int? TimeSpentMinutes { get; init; }
    public decimal? AutoScore { get; init; }
    public decimal? FinalScore { get; init; }
    public Guid? GradedBy { get; init; }
    public string? GradedByName { get; init; }
    public DateTime? GradedAt { get; init; }
    public ExamSubmissionStatus Status { get; init; }
}


