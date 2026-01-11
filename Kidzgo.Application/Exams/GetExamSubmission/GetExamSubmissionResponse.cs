using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.GetExamSubmission;

public sealed class GetExamSubmissionResponse
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
    public string? TeacherComment { get; init; }
    public ExamSubmissionStatus Status { get; init; }
    public List<SubmissionAnswerDto>? Answers { get; init; }
}

public sealed class SubmissionAnswerDto
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public int QuestionOrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public QuestionType QuestionType { get; init; }
    public string? QuestionOptions { get; init; }
    public string? QuestionCorrectAnswer { get; init; } // Only shown if ShowCorrectAnswers = true
    public int QuestionPoints { get; init; }
    public string Answer { get; init; } = null!;
    public bool IsCorrect { get; init; }
    public decimal? PointsAwarded { get; init; }
    public string? TeacherFeedback { get; init; }
    public DateTime? AnsweredAt { get; init; }
}


