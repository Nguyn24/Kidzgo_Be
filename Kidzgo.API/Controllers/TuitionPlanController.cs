using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.TuitionPlans.CreateTuitionPlan;
using Kidzgo.Application.TuitionPlans.DeleteTuitionPlan;
using Kidzgo.Application.TuitionPlans.GetTuitionPlanById;
using Kidzgo.Application.TuitionPlans.GetTuitionPlans;
using Kidzgo.Application.TuitionPlans.ToggleTuitionPlanStatus;
using Kidzgo.Application.TuitionPlans.UpdateTuitionPlan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/tuition-plans")]
[ApiController]
public class TuitionPlanController : ControllerBase
{
    private readonly ISender _mediator;

    public TuitionPlanController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-045: Tạo Tuition Plan
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> CreateTuitionPlan(
        [FromBody] CreateTuitionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTuitionPlanCommand
        {
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            Name = request.Name,
            TotalSessions = request.TotalSessions,
            TuitionAmount = request.TuitionAmount,
            UnitPriceSession = request.UnitPriceSession,
            Currency = request.Currency
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(tp => $"/api/tuition-plans/{tp.Id}");
    }

    /// <summary>
    /// UC-046: Xem danh sách Tuition Plans
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetTuitionPlans(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTuitionPlansQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Lấy tất cả Tuition Plans đang active (không cần authorize)
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IResult> GetActiveTuitionPlans(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTuitionPlansQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            IsActive = true, // Hardcode IsActive = true
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-047: Xem chi tiết Tuition Plan
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetTuitionPlanById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTuitionPlanByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-048: Cập nhật Tuition Plan
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> UpdateTuitionPlan(
        Guid id,
        [FromBody] UpdateTuitionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTuitionPlanCommand
        {
            Id = id,
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            Name = request.Name,
            TotalSessions = request.TotalSessions,
            TuitionAmount = request.TuitionAmount,
            UnitPriceSession = request.UnitPriceSession,
            Currency = request.Currency
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-049: Xóa mềm Tuition Plan
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteTuitionPlan(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTuitionPlanCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-050: Kích hoạt/Vô hiệu hóa Tuition Plan
    /// </summary>
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> ToggleTuitionPlanStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleTuitionPlanStatusCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

