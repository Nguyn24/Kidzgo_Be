using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.CreateExamQuestion;

public sealed class CreateExamQuestionCommandHandler(
    IDbContext context
) : ICommandHandler<CreateExamQuestionCommand, CreateExamQuestionResponse>
{
    public async Task<Result<CreateExamQuestionResponse>> Handle(
        CreateExamQuestionCommand command,
        CancellationToken cancellationToken)
    {
        // Check if exam exists
        var exam = await context.Exams
            .FirstOrDefaultAsync(e => e.Id == command.ExamId, cancellationToken);

        if (exam is null)
        {
            return Result.Failure<CreateExamQuestionResponse>(
                ExamQuestionErrors.ExamNotFound(command.ExamId));
        }

        // Validate question type and options
        if (command.QuestionType == QuestionType.MultipleChoice)
        {
            if (string.IsNullOrWhiteSpace(command.Options))
            {
                return Result.Failure<CreateExamQuestionResponse>(
                    ExamQuestionErrors.InvalidOptions);
            }

            if (string.IsNullOrWhiteSpace(command.CorrectAnswer))
            {
                return Result.Failure<CreateExamQuestionResponse>(
                    ExamQuestionErrors.MissingCorrectAnswer);
            }
        }

        var now = DateTime.UtcNow;
        var question = new ExamQuestion
        {
            Id = Guid.NewGuid(),
            ExamId = command.ExamId,
            OrderIndex = command.OrderIndex,
            QuestionText = command.QuestionText,
            QuestionType = command.QuestionType,
            Options = command.Options,
            CorrectAnswer = command.CorrectAnswer,
            Points = command.Points,
            Explanation = command.Explanation,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ExamQuestions.Add(question);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateExamQuestionResponse
        {
            Id = question.Id,
            ExamId = question.ExamId,
            OrderIndex = question.OrderIndex,
            QuestionText = question.QuestionText,
            QuestionType = question.QuestionType,
            Options = question.Options,
            CorrectAnswer = question.CorrectAnswer,
            Points = question.Points,
            Explanation = question.Explanation,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt
        };
    }
}


