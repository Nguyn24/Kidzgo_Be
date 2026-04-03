using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.NotificationTemplates;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.NotificationTemplates.GetAllNotificationTemplates;

public sealed class GetAllNotificationTemplatesQueryHandler(
    IDbContext context
) : IQueryHandler<GetAllNotificationTemplatesQuery, GetAllNotificationTemplatesResponse>
{
    public async Task<Result<GetAllNotificationTemplatesResponse>> Handle(
        GetAllNotificationTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        var templatesQuery = context.NotificationTemplates
            .AsNoTracking()
            .AsQueryable();

        // Filter by channel
        if (query.Channel.HasValue)
        {
            templatesQuery = templatesQuery.Where(t => t.Channel == query.Channel.Value);
        }

        // Filter by active status
        if (query.IsActive.HasValue)
        {
            templatesQuery = templatesQuery.Where(t => t.IsActive == query.IsActive.Value);
        }

        // Filter deleted
        if (query.IsDeleted.HasValue)
        {
            templatesQuery = templatesQuery.Where(t => t.IsDeleted == query.IsDeleted.Value);
        }
        else
        {
            // Default: exclude deleted templates
            templatesQuery = templatesQuery.Where(t => !t.IsDeleted);
        }

        // Get total count
        int totalCount = await templatesQuery.CountAsync(cancellationToken);

        // Apply pagination
        var templates = await templatesQuery
            .OrderByDescending(t => t.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(t => new NotificationTemplateDto
            {
                Id = t.Id,
                Code = t.Code,
                Channel = t.Channel,
                Title = t.Title,
                Content = t.Content,
                PlaceholdersRaw = t.Placeholders,
                Placeholders = ParsePlaceholders(t.Placeholders),
                IsActive = t.IsActive,
                IsDeleted = t.IsDeleted,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                Category = t.Category ?? NotificationTemplateContractMapper.InferCategory(t.Code, t.Channel),
                UsageCount = context.Notifications.Count(n => n.NotificationTemplateId == t.Id)
            })
            .ToListAsync(cancellationToken);

        var page = new Page<NotificationTemplateDto>(
            templates,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetAllNotificationTemplatesResponse
        {
            Templates = page
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

