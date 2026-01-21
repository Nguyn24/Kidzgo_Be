using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.MakeupCredits.CreateMakeupCredit;
using Kidzgo.Application.MakeupCredits.ExpireMakeupCredit;
using Kidzgo.Application.MakeupCredits.GetAllMakeupCredits;
using Kidzgo.Application.MakeupCredits.GetMakeupAllocations;
using Kidzgo.Application.MakeupCredits.GetMakeupCreditById;
using Kidzgo.Application.MakeupCredits.GetMakeupCredits;
using Kidzgo.Application.MakeupCredits.SuggestSessions;
using Kidzgo.Application.MakeupCredits.UseMakeupCredit;
using Kidzgo.Domain.Sessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/makeup-credits")]
[ApiController]
[Authorize]
public class MakeupController : ControllerBase
{
    private readonly ISender _mediator;

    public MakeupController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-109: Tạo Makeup Credit thủ công
    /// </summary>
    [HttpPost]
    public async Task<IResult> Create([FromBody] CreateMakeupCreditRequest request, CancellationToken cancellationToken)
    {
        Enum.TryParse<CreatedReason>(request.CreatedReason, true, out var createdReason);

        var command = new CreateMakeupCreditCommand
        {
            StudentProfileId = request.StudentProfileId,
            SourceSessionId = request.SourceSessionId,
            ExpiresAt = request.ExpiresAt,
            CreatedReason = createdReason == 0 ? CreatedReason.LongTerm : createdReason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/makeup-credits/{r.Id}");
    }

    /// <summary>
    /// UC-105/107: Danh sách Makeup Credits theo học sinh
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetList([FromQuery] Guid studentProfileId, CancellationToken cancellationToken)
    {
        var query = new GetMakeupCreditsQuery { StudentProfileId = studentProfileId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-105/107: Danh sach tat ca Makeup Credits
    /// </summary>
    [HttpGet("all")]
    public async Task<IResult> GetAll(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] string? status,
        [FromQuery] string? createdReason,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] DateTime? expiresFrom,
        [FromQuery] DateTime? expiresTo,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        MakeupCreditStatus? creditStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MakeupCreditStatus>(status, true, out var parsedStatus))
        {
            creditStatus = parsedStatus;
        }

        CreatedReason? parsedCreatedReason = null;
        if (!string.IsNullOrWhiteSpace(createdReason) &&
            Enum.TryParse<CreatedReason>(createdReason, true, out var parsedReason))
        {
            parsedCreatedReason = parsedReason;
        }

        var query = new GetAllMakeupCreditsQuery
        {
            StudentProfileId = studentProfileId,
            Status = creditStatus,
            CreatedReason = parsedCreatedReason,
            CreatedFrom = createdFrom,
            CreatedTo = createdTo,
            ExpiresFrom = expiresFrom,
            ExpiresTo = expiresTo,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-106: Chi tiết Makeup Credit
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetMakeupCreditByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-110/115: Đánh dấu sử dụng và phân bổ Makeup Credit
    /// </summary>
    [HttpPost("{id:guid}/use")]
    public async Task<IResult> Use(Guid id, [FromBody] UseMakeupCreditRequest request, CancellationToken cancellationToken)
    {
        var command = new UseMakeupCreditCommand
        {
            MakeupCreditId = id,
            TargetSessionId = request.TargetSessionId,
            AssignedBy = request.AssignedBy
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-111: Đánh dấu hết hạn Makeup Credit
    /// </summary>
    [HttpPost("{id:guid}/expire")]
    public async Task<IResult> Expire(Guid id, [FromBody] ExpireMakeupCreditRequest request, CancellationToken cancellationToken)
    {
        var command = new ExpireMakeupCreditCommand
        {
            MakeupCreditId = id,
            ExpiresAt = request.ExpiresAt
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-112/113: Đề xuất buổi bù
    /// </summary>
    [HttpGet("{id:guid}/suggestions")]
    public async Task<IResult> Suggest(Guid id, [FromQuery] int limit = 5, CancellationToken cancellationToken = default)
    {
        var query = new SuggestMakeupSessionsQuery
        {
            MakeupCreditId = id,
            Limit = limit
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-116: Danh sách Makeup Allocations theo học sinh
    /// </summary>
    [HttpGet("allocations")]
    public async Task<IResult> Allocations([FromQuery] Guid studentProfileId, CancellationToken cancellationToken)
    {
        var query = new GetMakeupAllocationsQuery { StudentProfileId = studentProfileId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
