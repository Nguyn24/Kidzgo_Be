using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.Notifications.GetBroadcastNotificationHistory;

public sealed class GetBroadcastNotificationHistoryResponse
{
    public Page<BroadcastNotificationHistoryDto> Broadcasts { get; init; } = null!;
}

public sealed class BroadcastNotificationHistoryDto
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public NotificationChannel Channel { get; init; }
    public string Title { get; init; } = null!;
    public string? Content { get; init; }
    public string? Deeplink { get; init; }
    public string? Kind { get; init; }
    public string? Priority { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public string? SenderRole { get; init; }
    public string? SenderName { get; init; }
    public string? TargetRole { get; init; }
    public int RecipientCount { get; init; }
    public int CreatedCount { get; init; }
    public int DeliveredCount { get; init; }
    public int PendingCount { get; init; }
    public int SentCount { get; init; }
    public int FailedCount { get; init; }
}
