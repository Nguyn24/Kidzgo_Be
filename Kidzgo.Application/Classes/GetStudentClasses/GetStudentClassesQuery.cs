using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Classes.GetStudentClasses;

public sealed class GetStudentClassesQuery : IQuery<GetStudentClassesResponse>, IPageableQuery
{
    public Guid StudentId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

