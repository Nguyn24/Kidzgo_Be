using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Exercises.GetExercises;

public sealed class GetExercisesResponse
{
    public Page<ExerciseDto> Exercises { get; init; } = null!;
}


