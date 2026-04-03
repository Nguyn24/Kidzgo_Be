using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.NotificationTemplates;
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
            PlaceholdersRaw = template.Placeholders,
            Placeholders = ParsePlaceholders(template.Placeholders),
            IsActive = template.IsActive,
            IsDeleted = template.IsDeleted,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Category = template.Category ?? NotificationTemplateContractMapper.InferCategory(template.Code, template.Channel),
            UsageCount = await context.Notifications.CountAsync(n => n.NotificationTemplateId == template.Id, cancellationToken)
        };
    }

    private static List<string> ParsePlaceholders(string? placeholders)
    {
        if (string.IsNullOrWhiteSpace(placeholders))
        {
            return [];
        }

        var trimmed = placeholders.Trim();
        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
        {
            trimmed = trimmed[1..^1];
        }

        return trimmed
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Trim().Trim('"'))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

