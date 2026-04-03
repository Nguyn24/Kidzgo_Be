using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.NotificationTemplates;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.NotificationTemplates.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommandHandler(
    IDbContext context
) : ICommandHandler<CreateNotificationTemplateCommand, CreateNotificationTemplateResponse>
{
    public async Task<Result<CreateNotificationTemplateResponse>> Handle(
        CreateNotificationTemplateCommand command,
        CancellationToken cancellationToken)
    {
        // Check if code already exists
        var existingTemplate = await context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Code == command.Code, cancellationToken);

        if (existingTemplate is not null)
        {
            return Result.Failure<CreateNotificationTemplateResponse>(
                NotificationTemplateErrors.CodeAlreadyExists(command.Code));
        }

        var now = DateTime.UtcNow;
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Code = command.Code,
            Channel = command.Channel,
            Title = command.Title,
            Content = command.Content,
            Placeholders = command.Placeholders,
            Category = command.Category ?? NotificationTemplateContractMapper.InferCategory(command.Code, command.Channel),
            IsActive = command.IsActive,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.NotificationTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateNotificationTemplateResponse
        {
            Id = template.Id,
            Code = template.Code,
            Channel = template.Channel,
            Title = template.Title,
            Content = template.Content,
            Placeholders = template.Placeholders,
            Category = template.Category,
            UsageCount = 0,
            IsActive = template.IsActive,
            IsDeleted = template.IsDeleted,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}

