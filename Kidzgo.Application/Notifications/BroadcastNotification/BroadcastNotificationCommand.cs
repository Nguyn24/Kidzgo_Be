using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.Notifications.BroadcastNotification;

public sealed class BroadcastNotificationCommand : ICommand<BroadcastNotificationResponse>
{
    public string Title { get; init; } = null!;
    public string? Content { get; init; }
    public string? Deeplink { get; init; }
    public NotificationChannel Channel { get; init; } = NotificationChannel.InApp;
    
    // Filters for recipients
    public string? Role { get; init; }  // Filter by user role
    public Guid? BranchId { get; init; }  // Filter by branch
    public Guid? ClassId { get; init; }  // Filter by class
    public Guid? StudentProfileId { get; init; }  // Filter by specific student
    public List<Guid>? UserIds { get; init; }  // Specific user IDs
    public List<Guid>? ProfileIds { get; init; }  // Specific profile IDs
}

