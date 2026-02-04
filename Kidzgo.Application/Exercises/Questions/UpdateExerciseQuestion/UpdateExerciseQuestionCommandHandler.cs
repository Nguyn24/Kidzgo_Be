using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Questions.UpdateExerciseQuestion;

public sealed class UpdateExerciseQuestionCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateExerciseQuestionCommand, UpdateExerciseQuestionResponse>
{
    public async Task<Result<UpdateExerciseQuestionResponse>> Handle(UpdateExerciseQuestionCommand command, CancellationToken cancellationToken)
    {
        var question = await context.ExerciseQuestions
            .FirstOrDefaultAsync(q => q.Id == command.QuestionId, cancellationToken);

        if (question is null)
        {
            return Result.Failure<UpdateExerciseQuestionResponse>(ExerciseErrors.QuestionNotFound(command.QuestionId));
        }

        var exercise = await context.Exercises
            .FirstOrDefaultAsync(e => e.Id == question.ExerciseId && !e.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure<UpdateExerciseQuestionResponse>(ExerciseErrors.NotFound(question.ExerciseId));
        }

        if (command.Points.HasValue && command.Points.Value < 0)
        {
            return Result.Failure<UpdateExerciseQuestionResponse>(ExerciseErrors.InvalidPoints);
        }

        if (command.OrderIndex.HasValue)
        {
            question.OrderIndex = command.OrderIndex.Value;
        }

        if (command.QuestionText is not null)
        {
            question.QuestionText = command.QuestionText;
        }

        if (command.QuestionType.HasValue)
        {
            question.QuestionType = command.QuestionType.Value;
        }

        if (command.Options is not null)
        {
            question.Options = command.Options;
        }

        if (command.CorrectAnswer is not null)
        {
            question.CorrectAnswer = command.CorrectAnswer;
        }

        if (command.Points.HasValue)
        {
            question.Points = command.Points.Value;
        }

        if (command.Explanation is not null)
        {
            question.Explanation = command.Explanation;
        }

        exercise.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new UpdateExerciseQuestionResponse { Id = question.Id };
    }
}


