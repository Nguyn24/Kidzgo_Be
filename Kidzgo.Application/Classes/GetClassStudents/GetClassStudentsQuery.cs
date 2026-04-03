using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Classes.GetClassStudents;

public sealed class GetClassStudentsQuery : IQuery<GetClassStudentsResponse>, IPageableQuery
{
    public Guid ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
