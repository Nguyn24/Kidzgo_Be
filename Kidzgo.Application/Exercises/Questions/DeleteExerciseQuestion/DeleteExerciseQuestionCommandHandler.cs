using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Questions.DeleteExerciseQuestion;

public sealed class DeleteExerciseQuestionCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteExerciseQuestionCommand>
{
    public async Task<Result> Handle(DeleteExerciseQuestionCommand command, CancellationToken cancellationToken)
    {
        var question = await context.ExerciseQuestions
            .FirstOrDefaultAsync(q => q.Id == command.QuestionId, cancellationToken);

        if (question is null)
        {
            return Result.Failure(ExerciseErrors.QuestionNotFound(command.QuestionId));
        }

        var exercise = await context.Exercises
            .FirstOrDefaultAsync(e => e.Id == question.ExerciseId && !e.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure(ExerciseErrors.NotFound(question.ExerciseId));
        }

        context.ExerciseQuestions.Remove(question);
        exercise.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}


