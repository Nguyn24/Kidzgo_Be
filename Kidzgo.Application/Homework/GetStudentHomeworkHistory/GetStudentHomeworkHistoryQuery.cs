using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Homework.GetStudentHomeworkHistory;

public sealed class GetStudentHomeworkHistoryQuery : IQuery<GetStudentHomeworkHistoryResponse>, IPageableQuery
{
    public Guid StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

