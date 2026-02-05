using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.NotificationTemplates.UpdateNotificationTemplate;

public sealed class UpdateNotificationTemplateCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateNotificationTemplateCommand, UpdateNotificationTemplateResponse>
{
    public async Task<Result<UpdateNotificationTemplateResponse>> Handle(
        UpdateNotificationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var template = await context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (template is null)
        {
            return Result.Failure<UpdateNotificationTemplateResponse>(
                NotificationTemplateErrors.NotFound(command.Id));
        }

        if (template.IsDeleted)
        {
            return Result.Failure<UpdateNotificationTemplateResponse>(
                NotificationTemplateErrors.Deleted);
        }

        template.Channel = command.Channel;
        template.Title = command.Title;
        template.Content = command.Content;
        template.Placeholders = command.Placeholders;
        template.IsActive = command.IsActive;
        template.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateNotificationTemplateResponse
        {
            Id = template.Id,
            Code = template.Code,
            Channel = template.Channel,
            Title = template.Title,
            Content = template.Content,
            Placeholders = template.Placeholders,
            IsActive = template.IsActive,
            IsDeleted = template.IsDeleted,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}

