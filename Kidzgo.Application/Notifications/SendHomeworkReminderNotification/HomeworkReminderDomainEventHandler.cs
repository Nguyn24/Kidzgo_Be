using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Notifications.SendHomeworkReminderNotification;

public sealed class HomeworkReminderDomainEventHandler(
    IDbContext context,
    IMailService mailService,
    ITemplateRenderer templateRenderer
) : INotificationHandler<HomeworkReminderDomainEvent>
{
    private const string TemplateCode = "HOMEWORK_REMINDER";

    public async Task Handle(HomeworkReminderDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == notification.RecipientUserId, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var template = await context.NotificationTemplates
            .Where(t => t.Code == TemplateCode && t.Channel == NotificationChannel.Email && t.IsActive && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return;
        }

        var placeholders = new Dictionary<string, string>
        {
            ["homework_title"] = notification.HomeworkTitle,
            ["due_date"] = notification.DueDate?.ToString("dd/MM/yyyy HH:mm") ?? "",
            ["class_name"] = notification.ClassName ?? ""
        };

        if (!string.IsNullOrWhiteSpace(template.Placeholders))
        {
            try
            {
                var templatePlaceholders = JsonSerializer.Deserialize<Dictionary<string, string>>(template.Placeholders);
                if (templatePlaceholders != null)
                {
                    foreach (var kvp in templatePlaceholders)
                    {
                        placeholders[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
                // Ignore JSON parse errors
            }
        }

        string subject = templateRenderer.Render(template.Title, placeholders);
        string body = templateRenderer.Render(template.Content ?? string.Empty, placeholders);

        await mailService.SendEmailAsync(user.Email, subject, body, cancellationToken);

        // Store homework ID in TemplateId to track which homework this reminder is for
        var notificationRecord = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = notification.RecipientUserId,
            RecipientProfileId = notification.RecipientProfileId,
            Channel = NotificationChannel.Email,
            Title = subject,
            Content = body,
            Status = NotificationStatus.Sent,
            SentAt = DateTime.UtcNow,
            TemplateId = notification.HomeworkId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        context.Notifications.Add(notificationRecord);
        await context.SaveChangesAsync(cancellationToken);
    }
}

