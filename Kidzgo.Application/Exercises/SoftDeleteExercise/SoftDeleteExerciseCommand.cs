using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.SoftDeleteExercise;

public sealed class SoftDeleteExerciseCommand : ICommand
{
    public Guid Id { get; init; }
}


