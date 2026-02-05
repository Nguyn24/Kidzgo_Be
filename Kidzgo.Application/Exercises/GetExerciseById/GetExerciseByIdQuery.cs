using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Exercises.GetExerciseById;

public sealed class GetExerciseByIdQuery : IQuery<GetExerciseByIdResponse>
{
    public Guid Id { get; init; }
}


