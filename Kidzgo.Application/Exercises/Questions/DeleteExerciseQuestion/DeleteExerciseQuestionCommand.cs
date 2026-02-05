using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.Questions.DeleteExerciseQuestion;

public sealed class DeleteExerciseQuestionCommand : ICommand
{
    public Guid QuestionId { get; init; }
}


