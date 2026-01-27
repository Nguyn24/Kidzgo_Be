namespace Kidzgo.Application.PlacementTests.ConvertLeadToEnrolled;

public sealed class ConvertLeadToEnrolledResponse
{
    public Guid LeadId { get; init; }
    public string LeadStatus { get; init; } = null!;
    public Guid PlacementTestId { get; init; }
    public string PlacementTestStatus { get; init; } = null!;
    public Guid? StudentProfileId { get; init; }
    public DateTime? ConvertedAt { get; init; }
}

