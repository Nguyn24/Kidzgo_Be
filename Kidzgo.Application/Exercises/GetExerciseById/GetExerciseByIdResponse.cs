namespace Kidzgo.Application.Exercises.GetExerciseById;

public sealed class GetExerciseByIdResponse
{
    public Guid Id { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? MissionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string ExerciseType { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public IReadOnlyList<ExerciseQuestionItem> Questions { get; init; } = Array.Empty<ExerciseQuestionItem>();
}

public sealed class ExerciseQuestionItem
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public string? Options { get; init; }
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
}


