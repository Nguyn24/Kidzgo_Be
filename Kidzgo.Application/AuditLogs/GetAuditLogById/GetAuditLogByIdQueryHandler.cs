using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AuditLogs;
using Kidzgo.Application.AuditLogs.GetAuditLogs;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetAuditLogByIdQuery, AuditLogDto>
{
    public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery query, CancellationToken cancellationToken)
    {
        var auditLog = await context.AuditLogs
            .AsNoTracking()
            .Include(a => a.ActorUser)
            .Include(a => a.ActorProfile)
            .Where(a => a.Id == query.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (auditLog is null)
        {
            return Result.Failure<AuditLogDto>(
                Error.NotFound("AuditLog.NotFound", $"Audit log with id '{query.Id}' was not found."));
        }

        return Result.Success(AuditLogContractMapper.ToDto(auditLog));
    }
}
