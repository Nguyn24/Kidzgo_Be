using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exercises.Questions.CreateExerciseQuestion;

public sealed class CreateExerciseQuestionCommandHandler(
    IDbContext context
) : ICommandHandler<CreateExerciseQuestionCommand, CreateExerciseQuestionResponse>
{
    public async Task<Result<CreateExerciseQuestionResponse>> Handle(
        CreateExerciseQuestionCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Points < 0)
        {
            return Result.Failure<CreateExerciseQuestionResponse>(ExerciseErrors.InvalidPoints);
        }

        var exercise = await context.Exercises
            .FirstOrDefaultAsync(e => e.Id == command.ExerciseId && !e.IsDeleted, cancellationToken);

        if (exercise is null)
        {
            return Result.Failure<CreateExerciseQuestionResponse>(ExerciseErrors.NotFound(command.ExerciseId));
        }

        var question = new ExerciseQuestion
        {
            Id = Guid.NewGuid(),
            ExerciseId = command.ExerciseId,
            OrderIndex = command.OrderIndex,
            QuestionText = command.QuestionText,
            QuestionType = command.QuestionType,
            Options = command.Options,
            CorrectAnswer = command.CorrectAnswer,
            Points = command.Points,
            Explanation = command.Explanation
        };

        context.ExerciseQuestions.Add(question);
        exercise.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new CreateExerciseQuestionResponse { Id = question.Id };
    }
}


