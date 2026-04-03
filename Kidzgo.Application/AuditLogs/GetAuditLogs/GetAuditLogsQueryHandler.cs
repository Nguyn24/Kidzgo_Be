using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.AuditLogs;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler(IDbContext context)
    : IQueryHandler<GetAuditLogsQuery, Page<AuditLogDto>>
{
    public async Task<Result<Page<AuditLogDto>>> Handle(GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        var auditLogsQuery = context.AuditLogs
            .AsNoTracking()
            .Include(a => a.ActorUser)
            .Include(a => a.ActorProfile)
            .AsQueryable();

        if (query.ActorId.HasValue)
        {
            auditLogsQuery = auditLogsQuery.Where(a =>
                a.ActorUserId == query.ActorId.Value || a.ActorProfileId == query.ActorId.Value);
        }

        if (query.ActorUserId.HasValue)
        {
            auditLogsQuery = auditLogsQuery.Where(a => a.ActorUserId == query.ActorUserId.Value);
        }

        if (query.ActorProfileId.HasValue)
        {
            auditLogsQuery = auditLogsQuery.Where(a => a.ActorProfileId == query.ActorProfileId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            var normalizedEntityType = query.EntityType.Trim().ToLower();
            auditLogsQuery = auditLogsQuery.Where(a => a.EntityType.ToLower() == normalizedEntityType);
        }

        if (query.EntityId.HasValue)
        {
            auditLogsQuery = auditLogsQuery.Where(a => a.EntityId == query.EntityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            var normalizedAction = query.Action.Trim().ToLower();
            auditLogsQuery = auditLogsQuery.Where(a => a.Action.ToLower() == normalizedAction);
        }

        if (query.FromDate.HasValue)
        {
            auditLogsQuery = auditLogsQuery.Where(a => a.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            auditLogsQuery = auditLogsQuery.Where(a => a.CreatedAt <= query.ToDate.Value);
        }

        var totalCount = await auditLogsQuery.CountAsync(cancellationToken);

        var auditLogs = await auditLogsQuery
            .OrderByDescending(a => a.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var logs = auditLogs
            .Select(AuditLogContractMapper.ToDto)
            .ToList();

        return Result.Success(new Page<AuditLogDto>(logs, totalCount, query.PageNumber, query.PageSize));
    }
}
