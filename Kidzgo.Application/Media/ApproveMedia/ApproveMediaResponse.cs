using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.ApproveMedia;

public sealed record ApproveMediaResponse
{
    public Guid Id { get; init; }
    public ApprovalStatus ApprovalStatus { get; init; }
    public Guid ApprovedById { get; init; }
    public string ApprovedByName { get; init; } = null!;
    public DateTime ApprovedAt { get; init; }
}

