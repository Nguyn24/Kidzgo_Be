using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank.GetQuestionBank;

public sealed class GetQuestionBankItemsQueryHandler(
    IDbContext context
) : IQueryHandler<GetQuestionBankItemsQuery, GetQuestionBankItemsResponse>
{
    public async Task<Result<GetQuestionBankItemsResponse>> Handle(
        GetQuestionBankItemsQuery query,
        CancellationToken cancellationToken)
    {
        var itemsQuery = context.QuestionBankItems
            .Where(q => q.ProgramId == query.ProgramId)
            .AsQueryable();

        if (query.Level.HasValue)
        {
            itemsQuery = itemsQuery.Where(q => q.Level == query.Level.Value);
        }

        var totalCount = await itemsQuery.CountAsync(cancellationToken);

        var entities = await itemsQuery
            .OrderByDescending(q => q.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(q => new QuestionBankItemDto
        {
            Id = q.Id,
            ProgramId = q.ProgramId,
            QuestionText = q.QuestionText,
            QuestionType = q.QuestionType.ToString(),
            Options = q.Options == null
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(q.Options) ?? new List<string>(),
            CorrectAnswer = q.CorrectAnswer,
            Points = q.Points,
            Explanation = q.Explanation,
            Level = q.Level,
            CreatedAt = q.CreatedAt
        }).ToList();

        var page = new Page<QuestionBankItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetQuestionBankItemsResponse
        {
            Items = page
        };
    }
}
