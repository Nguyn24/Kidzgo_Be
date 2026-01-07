using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Authentication.Profiles.RequestParentPinReset;

public sealed class ParentPinResetRequestDomainEventHandler(
    IDbContext context,
    IMailService mailService,
    ITemplateRenderer templateRenderer
) : INotificationHandler<ParentPinResetRequestDomainEvent>
{
    private const string TemplateCode = "PARENT_PIN_RESET";

    public async Task Handle(ParentPinResetRequestDomainEvent notification, CancellationToken cancellationToken)
    {
        Profile? profile = await context.Profiles
            .Include(p => p.User)
            .SingleOrDefaultAsync(p => p.Id == notification.ProfileId, cancellationToken);

        if (profile is null || profile.User is null || string.IsNullOrWhiteSpace(profile.User.Email))
        {
            return;
        }

        // Kiểm tra email template
        EmailTemplate? template = await context.EmailTemplates
            .Where(t => t.Code == TemplateCode && t.IsActive && !t.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            // Nếu không có template, có thể log warning nhưng không throw error
            return;
        }

        // Tạo token reset PIN có hạn sử dụng (1 giờ)
        var token = Guid.NewGuid().ToString("N");

        var resetToken = new ParentPinResetToken
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Xóa các token cũ chưa dùng của profile này
        var oldTokens = await context.ParentPinResetTokens
            .Where(t => t.ProfileId == profile.Id && !t.IsUsed)
            .ToListAsync(cancellationToken);

        if (oldTokens.Any())
        {
            context.ParentPinResetTokens.RemoveRange(oldTokens);
        }

        context.ParentPinResetTokens.Add(resetToken);
        await context.SaveChangesAsync(cancellationToken);

        // Render email template
        var placeholders = new Dictionary<string, string>
        {
            ["profile_name"] = profile.DisplayName,
            ["user_name"] = profile.User.Username ?? profile.User.Email,
            ["reset_link"] = $"https://kidzgo.app/parent/pin/reset?token={token}&profileId={profile.Id}"
        };

        string subject = template.Subject;
        string body = templateRenderer.Render(template.Body ?? string.Empty, placeholders);

        await mailService.SendEmailAsync(profile.User.Email, subject, body, cancellationToken);
    }
}

