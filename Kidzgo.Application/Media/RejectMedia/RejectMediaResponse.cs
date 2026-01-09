using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.RejectMedia;

public sealed record RejectMediaResponse
{
    public Guid Id { get; init; }
    public ApprovalStatus ApprovalStatus { get; init; }
    public DateTime UpdatedAt { get; init; }
}

