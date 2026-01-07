using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Classes.GetTeacherClasses;

public sealed class GetTeacherClassesQuery : IQuery<GetTeacherClassesResponse>, IPageableQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

