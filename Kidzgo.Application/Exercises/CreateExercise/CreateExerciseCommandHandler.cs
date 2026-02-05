using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exercises.CreateExercise;

public sealed class CreateExerciseCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateExerciseCommand, CreateExerciseResponse>
{
    public async Task<Result<CreateExerciseResponse>> Handle(
        CreateExerciseCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            MissionId = command.MissionId,
            Title = command.Title,
            Description = command.Description,
            ExerciseType = command.ExerciseType,
            CreatedBy = userContext.UserId,
            IsActive = command.IsActive,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateExerciseResponse { Id = exercise.Id };
    }
}


