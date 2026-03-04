using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AuditLogs.GetAuditLogs;

namespace Kidzgo.Application.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdQuery : IQuery<AuditLogDto>
{
    public Guid Id { get; init; }
}
