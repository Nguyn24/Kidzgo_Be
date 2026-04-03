using Kidzgo.Application.AuditLogs.GetAuditLogs;
using Kidzgo.Domain.Audit;

namespace Kidzgo.Application.AuditLogs;

internal static class AuditLogContractMapper
{
    public static AuditLogDto ToDto(AuditLog auditLog)
    {
        var actorUserName = auditLog.ActorUser != null
            ? (auditLog.ActorUser.Name ?? auditLog.ActorUser.Username ?? auditLog.ActorUser.Email)
            : null;

        return new AuditLogDto
        {
            Id = auditLog.Id,
            ActorUserId = auditLog.ActorUserId,
            ActorUserName = actorUserName,
            ActorProfileId = auditLog.ActorProfileId,
            ActorProfileName = auditLog.ActorProfile?.DisplayName,
            Action = auditLog.Action,
            EntityType = auditLog.EntityType,
            EntityId = auditLog.EntityId,
            DataBefore = auditLog.DataBefore,
            DataAfter = auditLog.DataAfter,
            CreatedAt = auditLog.CreatedAt,
            Role = auditLog.ActorUser?.Role.ToString() ?? auditLog.ActorProfile?.ProfileType.ToString(),
            IpAddress = auditLog.IpAddress,
            Details = auditLog.DataAfter ?? auditLog.DataBefore ?? auditLog.Action
        };
    }
}
