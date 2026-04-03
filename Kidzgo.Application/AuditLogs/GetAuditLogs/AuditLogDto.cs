namespace Kidzgo.Application.AuditLogs.GetAuditLogs;

public sealed class AuditLogDto
{
    public Guid Id { get; init; }
    public Guid? ActorUserId { get; init; }
    public string? ActorUserName { get; init; }
    public Guid? ActorProfileId { get; init; }
    public string? ActorProfileName { get; init; }
    public string Action { get; init; } = null!;
    public string EntityType { get; init; } = null!;
    public Guid? EntityId { get; init; }
    public string? DataBefore { get; init; }
    public string? DataAfter { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime Timestamp => CreatedAt;
    public string? User => ActorProfileName ?? ActorUserName;
    public string? Role { get; init; }
    public string Type => EntityType;
    public string? Reference => EntityId.HasValue ? $"{EntityType}:{EntityId.Value}" : EntityType;
    public string? IpAddress { get; init; }
    public string? Details { get; init; }
}
