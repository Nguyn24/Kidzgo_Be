using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.QuestionBank.GetQuestionBank;

public sealed class GetQuestionBankItemsQuery : IQuery<GetQuestionBankItemsResponse>
{
    public Guid ProgramId { get; init; }
    public QuestionLevel? Level { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class GetQuestionBankItemsResponse
{
    public Page<QuestionBankItemDto> Items { get; init; } = null!;
}
