using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.SoftDeleteExercise;

public sealed class SoftDeleteExerciseCommandHandler(
    IDbContext context
) : ICommandHandler<SoftDeleteExerciseCommand>
{
    public async Task<Result> Handle(SoftDeleteExerciseCommand command, CancellationToken cancellationToken)
    {
        var exercise = await context.Exercises
            .FirstOrDefaultAsync(e => e.Id == command.Id && !e.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure(ExerciseErrors.NotFound(command.Id));
        }

        exercise.IsDeleted = true;
        exercise.IsActive = false;
        exercise.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}


