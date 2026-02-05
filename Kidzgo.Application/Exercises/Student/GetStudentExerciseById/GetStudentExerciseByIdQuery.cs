using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Exercises.Student.GetStudentExerciseById;

/// <summary>
/// Student view of exercise details (hides correct answers).
/// UC-145: Học sinh làm Exercise
/// </summary>
public sealed class GetStudentExerciseByIdQuery : IQuery<GetStudentExerciseByIdResponse>
{
    public Guid ExerciseId { get; init; }
}


