using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.CRM;

namespace Kidzgo.Application.PlacementTests.GetPlacementTests;

public sealed class GetPlacementTestsQuery : IQuery<GetPlacementTestsResponse>, ISortableQuery
{
    public Guid? LeadId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public PlacementTestStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SortBy { get; init; }
    public SortOrder SortOrder { get; init; } = SortOrder.Descending;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

