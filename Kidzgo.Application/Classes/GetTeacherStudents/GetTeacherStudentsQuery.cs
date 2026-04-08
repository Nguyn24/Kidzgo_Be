using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Classes.GetTeacherStudents;

public sealed class GetTeacherStudentsQuery : IQuery<GetTeacherStudentsResponse>, IPageableQuery
{
    public Guid? ClassId { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
