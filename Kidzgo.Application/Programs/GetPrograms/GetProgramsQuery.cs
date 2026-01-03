using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Programs.GetPrograms;

public sealed class GetProgramsQuery : IQuery<GetProgramsResponse>, IPageableQuery
{
    public Guid? BranchId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

