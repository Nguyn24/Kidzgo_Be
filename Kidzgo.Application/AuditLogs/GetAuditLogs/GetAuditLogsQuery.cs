using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsQuery : IQuery<Page<AuditLogDto>>, IPageableQuery
{
    public Guid? ActorId { get; init; }
    public Guid? ActorUserId { get; init; }
    public Guid? ActorProfileId { get; init; }
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public string? Action { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
