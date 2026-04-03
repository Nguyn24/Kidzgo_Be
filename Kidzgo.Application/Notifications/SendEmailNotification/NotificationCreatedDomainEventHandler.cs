using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Notifications.SendEmailNotification;

public sealed class NotificationCreatedDomainEventHandler(
    IDbContext context,
    IMailService mailService,
    ITemplateRenderer templateRenderer
) : INotificationHandler<NotificationCreatedDomainEvent>
{
    public async Task Handle(NotificationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Only handle Email channel notifications
        if (notification.Channel != NotificationChannel.Email)
        {
            return;
        }

        var notificationRecord = await context.Notifications
            .Include(n => n.RecipientUser)
            .FirstOrDefaultAsync(n => n.Id == notification.NotificationId, cancellationToken);

        if (notificationRecord is null || notificationRecord.RecipientUser is null || 
            string.IsNullOrWhiteSpace(notificationRecord.RecipientUser.Email))
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

                    string subject = templateRenderer.Render(template.Title, placeholders);
                    string body = templateRenderer.Render(template.Content ?? string.Empty, placeholders);

                    await mailService.SendEmailAsync(notificationRecord.RecipientUser.Email, subject, body, cancellationToken);

                    notificationRecord.Status = NotificationStatus.Sent;
                    notificationRecord.SentAt = DateTime.UtcNow;
                }
                else
                {
                    // Template not found, use direct content
                    await mailService.SendEmailAsync(
                        notificationRecord.RecipientUser.Email,
                        notificationRecord.Title,
                        notificationRecord.Content ?? string.Empty,
                        cancellationToken);

                    notificationRecord.Status = NotificationStatus.Sent;
                    notificationRecord.SentAt = DateTime.UtcNow;
                }
            }
            else
            {
                // No template, use direct content
                await mailService.SendEmailAsync(
                    notificationRecord.RecipientUser.Email,
                    notificationRecord.Title,
                    notificationRecord.Content ?? string.Empty,
                    cancellationToken);

                notificationRecord.Status = NotificationStatus.Sent;
                notificationRecord.SentAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Mark as failed on error
            notificationRecord.Status = NotificationStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}

