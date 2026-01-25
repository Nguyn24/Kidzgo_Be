using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.MakeupCredits.GetStudentsWithMakeupOrLeave;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.MakeupCredits.GetParentStudentsWithMakeupOrLeave;

public sealed class GetParentStudentsWithMakeupOrLeaveQuery : IPageableQuery, IQuery<Page<StudentWithMakeupOrLeaveResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

