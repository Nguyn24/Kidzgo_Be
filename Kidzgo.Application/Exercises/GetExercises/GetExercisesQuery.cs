using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exercises.GetExercises;

public sealed class GetExercisesQuery : IQuery<GetExercisesResponse>, IPageableQuery
{
    public Guid? ClassId { get; init; }
    public Guid? MissionId { get; init; }
    public ExerciseType? ExerciseType { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}


