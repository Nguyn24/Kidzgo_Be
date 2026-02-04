namespace Kidzgo.Application.Exercises.Student.GetStudentExerciseById;

public sealed class GetStudentExerciseByIdResponse
{
    public Guid Id { get; init; }
    public Guid SubmissionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string ExerciseType { get; init; } = null!;
    public IReadOnlyList<StudentExerciseQuestionItem> Questions { get; init; } = Array.Empty<StudentExerciseQuestionItem>();
}

public sealed class StudentExerciseQuestionItem
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public string? Options { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
    public string? MyAnswer { get; init; }
}


