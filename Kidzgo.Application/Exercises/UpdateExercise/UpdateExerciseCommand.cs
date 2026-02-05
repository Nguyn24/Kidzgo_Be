using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exercises.UpdateExercise;

public sealed class UpdateExerciseCommand : ICommand<UpdateExerciseResponse>
{
    public Guid Id { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? MissionId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public ExerciseType? ExerciseType { get; init; }
    public bool? IsActive { get; init; }
}


