namespace Kidzgo.Application.Homework.CreateMultipleChoiceHomework;

public sealed class CreateMultipleChoiceHomeworkResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid? SessionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? DueAt { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool AllowResubmit { get; init; }
    public Guid? MissionId { get; init; }
    public string? Instructions { get; init; }
    public DateTime CreatedAt { get; init; }
    public int AssignedStudentsCount { get; init; }
    public List<HomeworkQuestionDto> Questions { get; init; } = new();
}

public class HomeworkQuestionDto
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; }= null!;
    public string QuestionType { get; init; } = null!;
    public List<string> Options { get; init; } = new();
    public int Points { get; init; }
}

