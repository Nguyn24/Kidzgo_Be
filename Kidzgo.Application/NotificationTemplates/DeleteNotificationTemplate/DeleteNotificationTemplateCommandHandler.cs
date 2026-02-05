using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.NotificationTemplates.DeleteNotificationTemplate;

public sealed class DeleteNotificationTemplateCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteNotificationTemplateCommand, DeleteNotificationTemplateResponse>
{
    public async Task<Result<DeleteNotificationTemplateResponse>> Handle(
        DeleteNotificationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var template = await context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (template is null)
        {
            return Result.Failure<DeleteNotificationTemplateResponse>(
                NotificationTemplateErrors.NotFound(command.Id));
        }

        if (template.IsDeleted)
        {
            return Result.Failure<DeleteNotificationTemplateResponse>(
                NotificationTemplateErrors.AlreadyDeleted);
        }

        // Soft delete
        template.IsDeleted = true;
        template.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteNotificationTemplateResponse
        {
            Id = template.Id,
            Code = template.Code,
            Title = template.Title,
            IsDeleted = template.IsDeleted,
            UpdatedAt = template.UpdatedAt
        };
    }
}

