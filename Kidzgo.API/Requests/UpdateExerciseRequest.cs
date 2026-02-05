using Kidzgo.Domain.Exams;

namespace Kidzgo.API.Requests;

public sealed class UpdateExerciseRequest
{
    public Guid? ClassId { get; init; }
    public Guid? MissionId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public ExerciseType? ExerciseType { get; init; }
    public bool? IsActive { get; init; }
}


