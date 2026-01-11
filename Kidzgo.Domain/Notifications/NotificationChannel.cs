namespace Kidzgo.Domain.Notifications;

public enum NotificationChannel
{
    InApp,  // In-app notification (stored in DB, read via API)
    ZaloOa,
    Push,
    Email
}
