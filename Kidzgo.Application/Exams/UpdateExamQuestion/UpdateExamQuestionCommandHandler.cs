using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.UpdateExamQuestion;

public sealed class UpdateExamQuestionCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateExamQuestionCommand, UpdateExamQuestionResponse>
{
    public async Task<Result<UpdateExamQuestionResponse>> Handle(
        UpdateExamQuestionCommand command,
        CancellationToken cancellationToken)
    {
        var question = await context.ExamQuestions
            .FirstOrDefaultAsync(q => q.Id == command.QuestionId, cancellationToken);

        if (question is null)
        {
            return Result.Failure<UpdateExamQuestionResponse>(
                ExamQuestionErrors.NotFound(command.QuestionId));
        }

        // Update fields if provided
        if (command.OrderIndex.HasValue)
        {
            question.OrderIndex = command.OrderIndex.Value;
        }

        if (!string.IsNullOrWhiteSpace(command.QuestionText))
        {
            question.QuestionText = command.QuestionText;
        }

        if (command.QuestionType.HasValue)
        {
            question.QuestionType = command.QuestionType.Value;
        }

        if (command.Options != null)
        {
            question.Options = command.Options;
        }

        if (command.CorrectAnswer != null)
        {
            question.CorrectAnswer = command.CorrectAnswer;
        }

        if (command.Points.HasValue)
        {
            question.Points = command.Points.Value;
        }

        if (command.Explanation != null)
        {
            question.Explanation = command.Explanation;
        }

        question.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new UpdateExamQuestionResponse
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


