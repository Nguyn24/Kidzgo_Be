using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AuditLogs.GetAuditLogs;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetAuditLogByIdQuery, AuditLogDto>
{
    public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery query, CancellationToken cancellationToken)
    {
        var log = await context.AuditLogs
            .AsNoTracking()
            .Include(a => a.ActorUser)
            .Include(a => a.ActorProfile)
            .Where(a => a.Id == query.Id)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                ActorUserId = a.ActorUserId,
                ActorUserName = a.ActorUser != null
                    ? (a.ActorUser.Name ?? a.ActorUser.Username ?? a.ActorUser.Email)
                    : null,
                ActorProfileId = a.ActorProfileId,
                ActorProfileName = a.ActorProfile != null ? a.ActorProfile.DisplayName : null,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                DataBefore = a.DataBefore,
                DataAfter = a.DataAfter,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null)
        {
            return Result.Failure<AuditLogDto>(
                Error.NotFound("AuditLog.NotFound", $"Audit log with id '{query.Id}' was not found."));
        }

        return Result.Success(log);
    }
}
