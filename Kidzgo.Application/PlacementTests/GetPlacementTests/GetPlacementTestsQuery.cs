using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.CRM;

namespace Kidzgo.Application.PlacementTests.GetPlacementTests;

public sealed class GetPlacementTestsQuery : IQuery<GetPlacementTestsResponse>
{
    public Guid? LeadId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public PlacementTestStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

