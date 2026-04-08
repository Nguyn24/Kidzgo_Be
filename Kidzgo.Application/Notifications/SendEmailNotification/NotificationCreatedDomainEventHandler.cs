using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Notifications.Shared;
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

            var subject = notificationRecord.Title;
            var body = notificationRecord.Content ?? string.Empty;
            var resolvedPlaceholders = NotificationPlaceholderResolver.ContainsPlaceholders(subject) ||
                                       NotificationPlaceholderResolver.ContainsPlaceholders(body)
                ? await NotificationPlaceholderResolver.ResolveAsync(context, notificationRecord, cancellationToken)
                : new Dictionary<string, string>();

            // If content was pre-rendered on the notification record, send it directly.
            if (!string.IsNullOrWhiteSpace(subject) &&
                (!string.IsNullOrWhiteSpace(notificationRecord.Content) || !templateId.HasValue))
            {
                if (resolvedPlaceholders.Count > 0)
                {
                    subject = templateRenderer.Render(subject, resolvedPlaceholders);
                    body = templateRenderer.Render(body, resolvedPlaceholders);
                }

                await mailService.SendEmailAsync(notificationRecord.RecipientUser.Email, subject, body, cancellationToken);

                notificationRecord.Status = NotificationStatus.Sent;
                notificationRecord.SentAt = VietnamTime.UtcNow();
            }
            else if (templateId.HasValue)
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

                    subject = templateRenderer.Render(template.Title, placeholders);
                    body = templateRenderer.Render(template.Content ?? string.Empty, placeholders);

                    await mailService.SendEmailAsync(notificationRecord.RecipientUser.Email, subject, body, cancellationToken);

                    notificationRecord.Status = NotificationStatus.Sent;
                    notificationRecord.SentAt = VietnamTime.UtcNow();
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
                    notificationRecord.SentAt = VietnamTime.UtcNow();
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
                notificationRecord.SentAt = VietnamTime.UtcNow();
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

