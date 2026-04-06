using Kidzgo.API.Extensions;
using Kidzgo.Application.AuditLogs.GetAuditLogById;
using Kidzgo.Application.AuditLogs.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/audit-logs")]
[ApiController]
[Authorize(Roles = "Admin,ManagementStaff")]
public class AuditLogController : ControllerBase
{
    private readonly ISender _mediator;

    public AuditLogController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Xem danh sách audit logs, có hỗ trợ filter theo actor và entity.
    /// </summary>
    [HttpGet]
    [HttpGet("/api/finance/audit-logs")]
    public async Task<IResult> GetAuditLogs(
        [FromQuery] Guid? actorId = null,
        [FromQuery] Guid? actorUserId = null,
        [FromQuery] Guid? actorProfileId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] string? action = null,
        [FromQuery(Name = "from")] DateTime? from = null,
        [FromQuery(Name = "to")] DateTime? to = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery
        {
            ActorId = actorId,
            ActorUserId = actorUserId,
            ActorProfileId = actorProfileId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            FromDate = from ?? fromDate,
            ToDate = to ?? toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Xem chi tiết 1 audit log (bao gồm data_before và data_after).
    /// </summary>
    [HttpGet("{id:guid}")]
    [HttpGet("/api/finance/audit-logs/{id:guid}")]
    public async Task<IResult> GetAuditLogById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }
}
