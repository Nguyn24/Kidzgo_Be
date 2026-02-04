using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exercises.Questions.CreateExerciseQuestion;

public sealed class CreateExerciseQuestionCommand : ICommand<CreateExerciseQuestionResponse>
{
    public Guid ExerciseId { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public QuestionType QuestionType { get; init; }
    public string? Options { get; init; }
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
}


