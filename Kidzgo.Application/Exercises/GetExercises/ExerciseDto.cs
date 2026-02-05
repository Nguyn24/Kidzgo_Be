namespace Kidzgo.Application.Exercises.GetExercises;

public sealed class ExerciseDto
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
    public int QuestionCount { get; init; }
}


