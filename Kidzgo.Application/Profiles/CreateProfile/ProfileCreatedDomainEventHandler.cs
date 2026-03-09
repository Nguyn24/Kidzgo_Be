using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Profiles.CreateProfile;

public sealed class ProfileCreatedDomainEventHandler(
    IDbContext context,
    IMailService mailService,
    ITemplateRenderer templateRenderer
) : INotificationHandler<ProfileCreatedDomainEvent>
{
    private const string TemplateCode = "PROFILE_CREATED";
    private const string FrontendUrl = "https://kidzgo-centre-pvjj.vercel.app/vi";
    private const string ApiUrl = "https://api.kidzgo.vn";

    public async Task Handle(ProfileCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.Email))
        {
            return;
        }

        EmailTemplate? template = await context.EmailTemplates
            .Where(t => t.Code == TemplateCode && t.IsActive && !t.IsDeleted)
            .SingleOrDefaultAsync(cancellationToken);

        if (template is null)
        {
            return;
        }

        var verifyLink = $"{FrontendUrl}/profile/verify?profileId={notification.ProfileId}";
        var updateLink = $"{ApiUrl}/api/profiles/{notification.ProfileId}/reactivate-and-update";

        var placeholders = new Dictionary<string, string>
        {
            ["profile_name"] = notification.DisplayName,
            ["profile_type"] = notification.ProfileType,
            ["email"] = notification.Email,
            ["phone"] = notification.Phone,
            ["full_name"] = notification.FullName,
            ["gender"] = notification.Gender,
            ["birth_day"] = notification.Birthday,
            ["zalo_id"] = notification.ZaloId,
            ["verify_link"] = verifyLink,
            ["update_link"] = updateLink,
            ["created_at"] = notification.CreatedAt
        };

        string subject = template.Subject;
        string body = templateRenderer.Render(template.Body ?? string.Empty, placeholders);

        await mailService.SendEmailAsync(notification.Email, subject, body, cancellationToken);
    }
}
