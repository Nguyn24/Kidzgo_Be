using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.MakeupCredits.GetStudentsWithMakeupOrLeave;

public sealed class GetStudentsWithMakeupOrLeaveQuery : IPageableQuery, IQuery<Page<StudentWithMakeupOrLeaveResponse>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public Guid? BranchId { get; init; }
}

