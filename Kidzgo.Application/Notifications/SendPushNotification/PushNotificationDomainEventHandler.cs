using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Notifications.Shared;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Kidzgo.Application.Notifications.SendPushNotification;

/// <summary>
/// UC-330: Gửi Notification qua Push real time
/// </summary>
public sealed class PushNotificationDomainEventHandler(
    IDbContext context,
    IPushNotificationService pushNotificationService,
    ITemplateRenderer templateRenderer,
    ILogger<PushNotificationDomainEventHandler> logger
) : INotificationHandler<NotificationCreatedDomainEvent>
{
    public async Task Handle(NotificationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Only handle Push channel notifications
        if (notification.Channel != NotificationChannel.Push)
        {
            return;
        }

        // Don't include RecipientUser to avoid enum conversion issues
        // We only need RecipientUserId which is already available
        var notificationRecord = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notification.NotificationId, cancellationToken);

        if (notificationRecord is null || notificationRecord.RecipientUserId == default(Guid))
        {
            return;
        }

        // Verify user exists and has valid role (skip if user has invalid role like 'Student')
        var userExists = await context.Users
            .Where(u => u.Id == notificationRecord.RecipientUserId)
            .Select(u => u.Id)
            .AnyAsync(cancellationToken);

        if (!userExists)
        {
            logger.LogWarning("User {UserId} not found for notification {NotificationId}", 
                notificationRecord.RecipientUserId, notification.NotificationId);
            notificationRecord.Status = NotificationStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            return;
        }

        // If notification already sent or failed, skip
        if (notificationRecord.Status != NotificationStatus.Pending)
        {
            return;
        }

        try
        {
            // Get active device tokens for the user
            var deviceTokens = await context.DeviceTokens
                .Where(dt => dt.UserId == notificationRecord.RecipientUserId && dt.IsActive)
                .Select(dt => dt.Token)
                .ToListAsync(cancellationToken);

            if (deviceTokens.Count == 0)
            {
                logger.LogWarning(
                    "No active device tokens found for user {UserId}. Push notification will not be sent.",
                    notificationRecord.RecipientUserId);
                notificationRecord.Status = NotificationStatus.Failed;
                await context.SaveChangesAsync(cancellationToken);
                return;
            }

            string title = notificationRecord.Title;
            string body = notificationRecord.Content ?? string.Empty;
            var resolvedPlaceholders = NotificationPlaceholderResolver.ContainsPlaceholders(title) ||
                                       NotificationPlaceholderResolver.ContainsPlaceholders(body)
                ? await NotificationPlaceholderResolver.ResolveAsync(context, notificationRecord, cancellationToken)
                : new Dictionary<string, string>();

            var templateId = notificationRecord.NotificationTemplateId;
            if (!templateId.HasValue &&
                !string.IsNullOrWhiteSpace(notificationRecord.TemplateId) &&
                Guid.TryParse(notificationRecord.TemplateId, out var parsedTemplateId))
            {
                templateId = parsedTemplateId;
            }

            // If content was pre-rendered on the notification record, keep it.
            if (string.IsNullOrWhiteSpace(title) && templateId.HasValue)
            {
                var template = await context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.Id == templateId.Value && t.IsActive && !t.IsDeleted, cancellationToken);

                if (template != null)
                {
                    var placeholders = new Dictionary<string, string>();

                    if (!string.IsNullOrWhiteSpace(template.Placeholders))
                    {
                        try
                        {
                            var templatePlaceholders = JsonSerializer.Deserialize<Dictionary<string, string>>(template.Placeholders);
                            if (templatePlaceholders != null)
                            {
                                placeholders = templatePlaceholders;
                            }
                        }
                        catch
                        {
                            // Ignore JSON parse errors
                        }
                    }

                    if (resolvedPlaceholders.Count > 0)
                    {
                        foreach (var placeholder in resolvedPlaceholders)
                        {
                            placeholders[placeholder.Key] = placeholder.Value;
                        }
                    }

                    title = templateRenderer.Render(template.Title, placeholders);
                    body = templateRenderer.Render(template.Content ?? string.Empty, placeholders);
                }
            }
            else if (resolvedPlaceholders.Count > 0)
            {
                title = templateRenderer.Render(title, resolvedPlaceholders);
                body = templateRenderer.Render(body, resolvedPlaceholders);
            }

            // Prepare data payload
            var data = new Dictionary<string, string>
            {
                ["notification_id"] = notificationRecord.Id.ToString(),
                ["type"] = "notification"
            };

            if (!string.IsNullOrWhiteSpace(notificationRecord.Deeplink))
            {
                data["deeplink"] = notificationRecord.Deeplink;
            }

            // Send push notification to all device tokens
            var results = await pushNotificationService.SendPushNotificationsAsync(
                deviceTokens,
                title,
                body,
                data,
                notificationRecord.Deeplink,
                cancellationToken);

            // Check if at least one notification was sent successfully
            var successCount = results.Values.Count(success => success);
            if (successCount > 0)
            {
                notificationRecord.Status = NotificationStatus.Sent;
                notificationRecord.SentAt = DateTime.UtcNow;
            }
            else
            {
                notificationRecord.Status = NotificationStatus.Failed;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Mark as failed on error
            notificationRecord.Status = NotificationStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            logger.LogError(ex, "Error sending push notification {NotificationId}", notificationRecord.Id);
            throw;
        }
    }
}

