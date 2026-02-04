using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exercises.CreateExercise;

public sealed class CreateExerciseCommand : ICommand<CreateExerciseResponse>
{
    public Guid? ClassId { get; init; }
    public Guid? MissionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public ExerciseType ExerciseType { get; init; }
    public bool IsActive { get; init; } = true;
}


