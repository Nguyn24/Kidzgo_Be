using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Notifications.SendSessionReminderNotification;

public sealed class SessionReminderDomainEventHandler(
    IDbContext context,
    IMailService mailService,
    ITemplateRenderer templateRenderer
) : INotificationHandler<SessionReminderDomainEvent>
{
    private const string TemplateCode = "SESSION_REMINDER";

    public async Task Handle(SessionReminderDomainEvent notification, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == notification.RecipientUserId, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        // Get notification template
        var template = await context.NotificationTemplates
            .Where(t => t.Code == TemplateCode && t.Channel == NotificationChannel.Email && t.IsActive && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return;
        }

        // Parse placeholders if available
        var placeholders = new Dictionary<string, string>
        {
            ["session_title"] = notification.SessionTitle,
            ["session_start_time"] = notification.SessionStartTime.ToString("dd/MM/yyyy HH:mm"),
            ["class_name"] = notification.ClassName ?? "",
            ["location"] = notification.Location ?? "",
            ["student_name"] = notification.StudentName ?? "",
            ["classroom_name"] = notification.ClassroomName ?? ""
        };

        // If template has placeholders JSON, merge with defaults
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

        // Render template
        string subject = templateRenderer.Render(template.Title, placeholders);
        string body = templateRenderer.Render(template.Content ?? string.Empty, placeholders);

        // Send email
        await mailService.SendEmailAsync(user.Email, subject, body, cancellationToken);

        // Create notification record
        // Store session ID in TemplateId to track which session this reminder is for
        var notificationRecord = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = notification.RecipientUserId,
            RecipientProfileId = notification.RecipientProfileId,
            Channel = NotificationChannel.Email,
            Title = subject,
            Content = body,
            Status = NotificationStatus.Sent,
            SentAt = VietnamTime.UtcNow(),
            NotificationTemplateId = template.Id,
            TemplateId = notification.SessionId.ToString(), // Store session ID to avoid duplicates
            CreatedAt = VietnamTime.UtcNow()
        };

        context.Notifications.Add(notificationRecord);
        await context.SaveChangesAsync(cancellationToken);
    }
}

