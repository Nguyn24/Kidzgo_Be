using System.Reflection;
using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(Kidzgo.Infrastructure.Database.ApplicationDbContext))]
    [Migration("20260402170000_ReseedNotificationAndEmailTemplates")]
    public partial class ReseedNotificationAndEmailTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var template in GetSeedEntities<EmailTemplate>())
            {
                migrationBuilder.Sql(BuildEmailTemplateUpsertSql(template));
            }

            foreach (var template in GetSeedEntities<NotificationTemplate>())
            {
                migrationBuilder.Sql(BuildNotificationTemplateUpsertSql(template));
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var emailCodes = string.Join(", ", GetSeedEntities<EmailTemplate>().Select(template => SqlLiteral(template.Code)));
            var notificationCodes = string.Join(", ", GetSeedEntities<NotificationTemplate>().Select(template => SqlLiteral(template.Code)));

            migrationBuilder.Sql($"""
                DELETE FROM public."EmailTemplates"
                WHERE "Code" IN ({emailCodes});
                """);

            migrationBuilder.Sql($"""
                DELETE FROM public."NotificationTemplates"
                WHERE "Code" IN ({notificationCodes});
                """);
        }

        private static IEnumerable<TEntity> GetSeedEntities<TEntity>()
            where TEntity : class, new()
        {
            var snapshot = new ApplicationDbContextModelSnapshot();
            var entityType = snapshot.Model.FindEntityType(typeof(TEntity))
                ?? throw new InvalidOperationException($"Seed metadata for {typeof(TEntity).Name} was not found in model snapshot.");

            return entityType.GetSeedData().Select(MapSeedData<TEntity>).ToList();
        }

        private static TEntity MapSeedData<TEntity>(IDictionary<string, object> seedData)
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

        private static object ConvertValue(object value, Type targetType)
        {
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

            return value;
        }

        private static string BuildEmailTemplateUpsertSql(EmailTemplate template)
        {
            return $"""
                INSERT INTO public."EmailTemplates" (
                    "Id", "Code", "Subject", "Body", "Placeholders",
                    "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
                VALUES (
                    {SqlLiteral(template.Id)},
                    {SqlLiteral(template.Code)},
                    {SqlLiteral(template.Subject)},
                    {SqlLiteral(template.Body)},
                    {SqlLiteral(template.Placeholders)},
                    {SqlLiteral(template.IsActive)},
                    {SqlLiteral(template.IsDeleted)},
                    {SqlLiteral(template.CreatedAt)},
                    {SqlLiteral(template.UpdatedAt)})
                ON CONFLICT ("Code") DO UPDATE
                SET
                    "Subject" = EXCLUDED."Subject",
                    "Body" = EXCLUDED."Body",
                    "Placeholders" = EXCLUDED."Placeholders",
                    "IsActive" = EXCLUDED."IsActive",
                    "IsDeleted" = EXCLUDED."IsDeleted",
                    "UpdatedAt" = EXCLUDED."UpdatedAt";
                """;
        }

        private static string BuildNotificationTemplateUpsertSql(NotificationTemplate template)
        {
            return $"""
                INSERT INTO public."NotificationTemplates" (
                    "Id", "Code", "Channel", "Title", "Content", "Placeholders",
                    "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
                VALUES (
                    {SqlLiteral(template.Id)},
                    {SqlLiteral(template.Code)},
                    {SqlLiteral(template.Channel.ToString())},
                    {SqlLiteral(template.Title)},
                    {SqlLiteral(template.Content)},
                    {SqlLiteral(template.Placeholders)},
                    {SqlLiteral(template.IsActive)},
                    {SqlLiteral(template.IsDeleted)},
                    {SqlLiteral(template.CreatedAt)},
                    {SqlLiteral(template.UpdatedAt)})
                ON CONFLICT ("Code") DO UPDATE
                SET
                    "Channel" = EXCLUDED."Channel",
                    "Title" = EXCLUDED."Title",
                    "Content" = EXCLUDED."Content",
                    "Placeholders" = EXCLUDED."Placeholders",
                    "IsActive" = EXCLUDED."IsActive",
                    "IsDeleted" = EXCLUDED."IsDeleted",
                    "UpdatedAt" = EXCLUDED."UpdatedAt";
                """;
        }

        private static string SqlLiteral(Guid value) => $"'{value}'";

        private static string SqlLiteral(bool value) => value ? "TRUE" : "FALSE";

        private static string SqlLiteral(DateTime value) => $"TIMESTAMPTZ '{value:O}'";

        private static string SqlLiteral(string value) => $"'{value.Replace("'", "''")}'";

        private static string SqlLiteral(string valueOrNull, bool allowNull = true)
        {
            return valueOrNull is null
                ? "NULL"
                : $"'{valueOrNull.Replace("'", "''")}'";
        }
    }
}
