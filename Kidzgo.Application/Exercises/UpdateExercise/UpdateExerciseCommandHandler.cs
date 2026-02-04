using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.UpdateExercise;

public sealed class UpdateExerciseCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateExerciseCommand, UpdateExerciseResponse>
{
    public async Task<Result<UpdateExerciseResponse>> Handle(UpdateExerciseCommand command, CancellationToken cancellationToken)
    {
        var exercise = await context.Exercises
            .FirstOrDefaultAsync(e => e.Id == command.Id && !e.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure<UpdateExerciseResponse>(ExerciseErrors.NotFound(command.Id));
        }

        if (command.ClassId.HasValue)
        {
            exercise.ClassId = command.ClassId;
        }

        if (command.MissionId.HasValue)
        {
            exercise.MissionId = command.MissionId;
        }

        if (command.Title is not null)
        {
            exercise.Title = command.Title;
        }

        if (command.Description is not null)
        {
            exercise.Description = command.Description;
        }

        if (command.ExerciseType.HasValue)
        {
            exercise.ExerciseType = command.ExerciseType.Value;
        }

        if (command.IsActive.HasValue)
        {
            exercise.IsActive = command.IsActive.Value;
        }

        exercise.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateExerciseResponse { Id = exercise.Id };
    }
}


