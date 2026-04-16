using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.ResubmitMedia;

public sealed record ResubmitMediaResponse
{
    public Guid Id { get; init; }
    public ApprovalStatus ApprovalStatus { get; init; }
    public bool IsPublished { get; init; }
    public DateTime UpdatedAt { get; init; }
}
