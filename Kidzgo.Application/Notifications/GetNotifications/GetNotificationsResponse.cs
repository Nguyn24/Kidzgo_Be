using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.Notifications.GetNotifications;

public sealed class GetNotificationsResponse
{
    public Page<NotificationDto> Notifications { get; init; } = null!;
}

public sealed class NotificationDto
{
    public Guid Id { get; init; }
    public Guid RecipientUserId { get; init; }
    public Guid? RecipientProfileId { get; init; }
    public NotificationChannel Channel { get; init; }
    public string Title { get; init; } = null!;
    public string? Content { get; init; }
    public string? Deeplink { get; init; }
    public NotificationStatus Status { get; init; }
    public DateTime? SentAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsRead { get; init; }
}

