using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.MakeupCredits.CreateMakeupCredit;
using Kidzgo.Application.MakeupCredits.ExpireMakeupCredit;
using Kidzgo.Application.MakeupCredits.GetAllMakeupCredits;
using Kidzgo.Application.MakeupCredits.GetMakeupAllocations;
using Kidzgo.Application.MakeupCredits.GetMakeupCreditById;
using Kidzgo.Application.MakeupCredits.GetMakeupCredits;
using Kidzgo.Application.MakeupCredits.GetStudentsWithMakeupOrLeave;
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

    /// Tạo Makeup Credit thủ công
    // [HttpPost]
    // public async Task<IResult> Create([FromBody] CreateMakeupCreditRequest request, CancellationToken cancellationToken)
    // {
    //     Enum.TryParse<CreatedReason>(request.CreatedReason, true, out var createdReason);
    //
    //     var command = new CreateMakeupCreditCommand
    //     {
    //         StudentProfileId = request.StudentProfileId,
    //         SourceSessionId = request.SourceSessionId,
    //         ExpiresAt = request.ExpiresAt,
    //         CreatedReason = createdReason == 0 ? CreatedReason.LongTerm : createdReason
    //     };
    //
    //     var result = await _mediator.Send(command, cancellationToken);
    //     return result.MatchCreated(r => $"/api/makeup-credits/{r.Id}");
    // }

    /// UC-105/107: Danh sách Makeup Credits theo học sinh
    [HttpGet]
    public async Task<IResult> GetList([FromQuery] Guid studentProfileId, CancellationToken cancellationToken)
    {
        var query = new GetMakeupCreditsQuery { StudentProfileId = studentProfileId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-105/107: Danh sach tat ca Makeup Credits
    [HttpGet("all")]
    public async Task<IResult> GetAll(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] MakeupCreditStatus? status,
        [FromQuery] Guid? branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllMakeupCreditsQuery
        {
            StudentProfileId = studentProfileId,
            Status = status,
            BranchId = branchId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-106: Chi tiết Makeup Credit
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetMakeupCreditByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-110/115: Đánh dấu sử dụng và phân bổ Makeup Credit
    [HttpPost("{id:guid}/use")]
    public async Task<IResult> Use(Guid id, [FromBody] UseMakeupCreditRequest request, CancellationToken cancellationToken)
    {
        var command = new UseMakeupCreditCommand
        {
            MakeupCreditId = id,
            ClassId = request.ClassId,
            TargetSessionId = request.TargetSessionId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-111: Đánh dấu hết hạn Makeup Credit
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

    /// UC-112/113: Đề xuất buổi bù
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

    /// UC-116: Danh sách Makeup Allocations theo học sinh
    [HttpGet("allocations")]
    public async Task<IResult> Allocations([FromQuery] Guid studentProfileId, CancellationToken cancellationToken)
    {
        var query = new GetMakeupAllocationsQuery { StudentProfileId = studentProfileId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Danh sách học viên đang có đơn xin nghỉ hoặc đang có makeup credit
    /// <param name="searchTerm">Search by student display name</param>
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet("students")]
    public async Task<IResult> GetStudentsWithMakeupOrLeave(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentsWithMakeupOrLeaveQuery
        {
            SearchTerm = searchTerm,
            BranchId = branchId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
