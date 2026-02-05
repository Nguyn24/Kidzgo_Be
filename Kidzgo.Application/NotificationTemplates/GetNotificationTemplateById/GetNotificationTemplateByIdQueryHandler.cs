using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.NotificationTemplates.GetNotificationTemplateById;

public sealed class GetNotificationTemplateByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetNotificationTemplateByIdQuery, GetNotificationTemplateByIdResponse>
{
    public async Task<Result<GetNotificationTemplateByIdResponse>> Handle(
        GetNotificationTemplateByIdQuery query,
        CancellationToken cancellationToken)
    {
        var template = await context.NotificationTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken);

        if (template is null)
        {
            return Result.Failure<GetNotificationTemplateByIdResponse>(
                NotificationTemplateErrors.NotFound(query.Id));
        }

        return new GetNotificationTemplateByIdResponse
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

