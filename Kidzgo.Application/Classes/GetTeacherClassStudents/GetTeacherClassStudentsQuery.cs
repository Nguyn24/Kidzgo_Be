using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.GetTeacherClassStudents;

public sealed class GetTeacherClassStudentsQuery : IQuery<GetTeacherClassStudentsResponse>
{
    public Guid ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}


