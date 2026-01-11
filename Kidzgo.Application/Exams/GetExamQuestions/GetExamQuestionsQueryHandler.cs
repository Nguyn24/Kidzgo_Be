using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExamQuestions;

public sealed class GetExamQuestionsQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamQuestionsQuery, GetExamQuestionsResponse>
{
    public async Task<Result<GetExamQuestionsResponse>> Handle(
        GetExamQuestionsQuery query,
        CancellationToken cancellationToken)
    {
        // Check if exam exists
        var examExists = await context.Exams
            .AnyAsync(e => e.Id == query.ExamId, cancellationToken);

        if (!examExists)
        {
            return Result.Failure<GetExamQuestionsResponse>(
                ExamQuestionErrors.ExamNotFound(query.ExamId));
        }

        var questions = await context.ExamQuestions
            .Where(q => q.ExamId == query.ExamId)
            .OrderBy(q => q.OrderIndex)
            .Select(q => new ExamQuestionDto
            {
                Id = q.Id,
                ExamId = q.ExamId,
                OrderIndex = q.OrderIndex,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                Points = q.Points,
                Explanation = q.Explanation,
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetExamQuestionsResponse
        {
            Questions = questions
        };
    }
}


