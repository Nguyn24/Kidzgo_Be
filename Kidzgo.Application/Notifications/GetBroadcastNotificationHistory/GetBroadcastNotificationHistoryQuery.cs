using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.Notifications.GetBroadcastNotificationHistory;

public sealed class GetBroadcastNotificationHistoryQuery : IQuery<GetBroadcastNotificationHistoryResponse>
{
    public NotificationChannel? Channel { get; init; }
    public string? SenderRole { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
