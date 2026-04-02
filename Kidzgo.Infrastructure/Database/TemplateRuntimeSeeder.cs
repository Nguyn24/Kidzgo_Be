using System.Reflection;
using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.Database;

public sealed class TemplateRuntimeSeeder(
    ApplicationDbContext dbContext,
    ILogger<TemplateRuntimeSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var designTimeModel = dbContext.GetService<IDesignTimeModel>().Model;

        var emailSeeds = GetSeedEntities<EmailTemplate>(designTimeModel);
        var notificationSeeds = GetSeedEntities<NotificationTemplate>(designTimeModel);

        var existingEmailCodes = await dbContext.EmailTemplates
            .AsNoTracking()
            .Select(template => template.Code)
            .ToHashSetAsync(cancellationToken);

        var existingNotificationCodes = await dbContext.NotificationTemplates
            .AsNoTracking()
            .Select(template => template.Code)
            .ToHashSetAsync(cancellationToken);

        var missingEmailTemplates = emailSeeds
            .Where(template => !existingEmailCodes.Contains(template.Code))
            .ToList();

        var missingNotificationTemplates = notificationSeeds
            .Where(template => !existingNotificationCodes.Contains(template.Code))
            .ToList();

        if (missingEmailTemplates.Count == 0 && missingNotificationTemplates.Count == 0)
        {
            logger.LogInformation("Template runtime seeder found no missing template rows.");
            return;
        }

        if (missingEmailTemplates.Count > 0)
        {
            dbContext.EmailTemplates.AddRange(missingEmailTemplates);
        }

        if (missingNotificationTemplates.Count > 0)
        {
            dbContext.NotificationTemplates.AddRange(missingNotificationTemplates);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Template runtime seeder inserted {EmailCount} email template(s) and {NotificationCount} notification template(s).",
            missingEmailTemplates.Count,
            missingNotificationTemplates.Count);
    }

    private static List<TEntity> GetSeedEntities<TEntity>(IModel model)
        where TEntity : class, new()
    {
        var entityType = model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Seed metadata for {typeof(TEntity).Name} was not found.");

        return entityType.GetSeedData()
            .Select(MapSeedData<TEntity>)
            .ToList();
    }

    private static TEntity MapSeedData<TEntity>(IDictionary<string, object?> seedData)
        where TEntity : class, new()
    {
        var entity = new TEntity();

        foreach (var entry in seedData)
        {
            var property = typeof(TEntity).GetProperty(
                entry.Key,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null || !property.CanWrite)
            {
                continue;
            }

            property.SetValue(entity, ConvertValue(entry.Value, property.PropertyType));
        }

        return entity;
    }

    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
        {
            return null;
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType.IsEnum)
        {
            return value is string enumString
                ? Enum.Parse(underlyingType, enumString)
                : Enum.ToObject(underlyingType, value);
        }

        if (underlyingType == typeof(Guid) && value is string guidString)
        {
            return Guid.Parse(guidString);
        }

        if (underlyingType == typeof(DateTime) && value is string dateTimeString)
        {
            return DateTime.Parse(dateTimeString);
        }

        if (underlyingType == typeof(DateOnly) && value is string dateOnlyString)
        {
            return DateOnly.Parse(dateOnlyString);
        }

        return value;
    }
}
