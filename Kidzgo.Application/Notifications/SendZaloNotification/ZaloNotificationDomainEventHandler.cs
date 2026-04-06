using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;

namespace Kidzgo.Application.Notifications.SendZaloNotification;

/// <summary>
/// UC-328: Gửi Notification qua Zalo OA
/// </summary>
public sealed class ZaloNotificationDomainEventHandler(
    IDbContext context,
    IZaloService zaloService,
    ITemplateRenderer templateRenderer,
    ILogger<ZaloNotificationDomainEventHandler> logger
) : INotificationHandler<NotificationCreatedDomainEvent>
{
    public async Task Handle(NotificationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Only handle ZaloOa channel notifications
        if (notification.Channel != NotificationChannel.ZaloOa)
        {
            return;
        }

        var notificationRecord = await context.Notifications
            .Include(n => n.RecipientUser)
            .FirstOrDefaultAsync(n => n.Id == notification.NotificationId, cancellationToken);

        if (notificationRecord is null || notificationRecord.RecipientUser is null)
        {
            return;
        }

        // If notification already sent or failed, skip
        if (notificationRecord.Status != NotificationStatus.Pending)
        {
            return;
        }

        try
        {
            // Get Zalo user ID from Lead (if user has associated lead with ZaloId)
            // Or from User profile if we add ZaloUserId field later
            var zaloUserId = await GetZaloUserIdAsync(notificationRecord.RecipientUserId, cancellationToken);

            if (string.IsNullOrWhiteSpace(zaloUserId))
            {
                logger.LogWarning(
                    "No Zalo user ID found for user {UserId}. Zalo notification will not be sent.",
                    notificationRecord.RecipientUserId);
                notificationRecord.Status = NotificationStatus.Failed;
                await context.SaveChangesAsync(cancellationToken);
                return;
            }

            string message = notificationRecord.Content ?? notificationRecord.Title;

            var templateId = notificationRecord.NotificationTemplateId;
            if (!templateId.HasValue &&
                !string.IsNullOrWhiteSpace(notificationRecord.TemplateId) &&
                Guid.TryParse(notificationRecord.TemplateId, out var parsedTemplateId))
            {
                templateId = parsedTemplateId;
            }

            // If template ID is provided, use template
            if (templateId.HasValue)
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

                    message = templateRenderer.Render(template.Content ?? template.Title, placeholders);
                }
            }

            // Send Zalo message
            var success = await zaloService.SendMessageWithDeeplinkAsync(
                zaloUserId,
                message,
                notificationRecord.Deeplink,
                cancellationToken);

            if (success)
            {
                notificationRecord.Status = NotificationStatus.Sent;
                notificationRecord.SentAt = DateTime.UtcNow;
                logger.LogInformation(
                    "Zalo notification sent successfully to user {UserId} (ZaloId: {ZaloUserId})",
                    notificationRecord.RecipientUserId, zaloUserId);
            }
            else
            {
                notificationRecord.Status = NotificationStatus.Failed;
                logger.LogWarning(
                    "Failed to send Zalo notification to user {UserId} (ZaloId: {ZaloUserId})",
                    notificationRecord.RecipientUserId, zaloUserId);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending Zalo notification to user {UserId}", notificationRecord.RecipientUserId);
            notificationRecord.Status = NotificationStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string?> GetZaloUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        // Get user info to match with Lead
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        // Try to get ZaloId from Lead by matching phone or email
        // This assumes that when a user is created from a Zalo lead, the contact info matches
        var lead = await context.Leads
            .Where(l => !string.IsNullOrWhiteSpace(l.ZaloId) &&
                       ((!string.IsNullOrWhiteSpace(user.PhoneNumber) && l.Phone == user.PhoneNumber) ||
                        (!string.IsNullOrWhiteSpace(user.Email) && l.Email == user.Email) ||
                        l.OwnerStaffId == userId))
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (lead != null && !string.IsNullOrWhiteSpace(lead.ZaloId))
        {
            return lead.ZaloId;
        }

        // TODO: If User table has ZaloUserId field in the future, get it from there
        // return user.ZaloUserId;

        return null;
    }
}

