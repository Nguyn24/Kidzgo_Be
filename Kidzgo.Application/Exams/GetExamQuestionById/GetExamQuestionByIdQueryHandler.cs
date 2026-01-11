using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExamQuestionById;

public sealed class GetExamQuestionByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamQuestionByIdQuery, GetExamQuestionByIdResponse>
{
    public async Task<Result<GetExamQuestionByIdResponse>> Handle(
        GetExamQuestionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var question = await context.ExamQuestions
            .FirstOrDefaultAsync(q => q.Id == query.QuestionId, cancellationToken);

        if (question is null)
        {
            return Result.Failure<GetExamQuestionByIdResponse>(
                ExamQuestionErrors.NotFound(query.QuestionId));
        }

        return new GetExamQuestionByIdResponse
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


