using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Programs.CreateProgram;
using Kidzgo.Application.Programs.DeleteProgram;
using Kidzgo.Application.Programs.GetProgramById;
using Kidzgo.Application.Programs.GetPrograms;
using Kidzgo.Application.Programs.ToggleProgramStatus;
using Kidzgo.Application.Programs.UpdateProgram;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/programs")]
[ApiController]
public class ProgramController : ControllerBase
{
    private readonly ISender _mediator;

    public ProgramController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-039: Tạo Program
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateProgram(
        [FromBody] CreateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProgramCommand
        {
            BranchId = request.BranchId,
            Name = request.Name,
            Level = request.Level,
            TotalSessions = request.TotalSessions,
            DefaultTuitionAmount = request.DefaultTuitionAmount,
            UnitPriceSession = request.UnitPriceSession,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(p => $"/api/programs/{p.Id}");
    }

    /// UC-040: Xem danh sách Programs
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetPrograms(
        [FromQuery] Guid? branchId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramsQuery
        {
            BranchId = branchId,
            SearchTerm = searchTerm,
            IsActive = isActive,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lấy danh sách tất cả Programs đang active (IsActive = true)
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IResult> GetActivePrograms(
        [FromQuery] Guid? branchId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramsQuery
        {
            BranchId = branchId,
            SearchTerm = searchTerm,
            IsActive = true, // Chỉ lấy programs active
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-041: Xem chi tiết Program
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetProgramById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProgramByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-042: Cập nhật Program
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateProgram(
        Guid id,
        [FromBody] UpdateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProgramCommand
        {
            Id = id,
            BranchId = request.BranchId,
            Name = request.Name,
            Level = request.Level,
            TotalSessions = request.TotalSessions,
            DefaultTuitionAmount = request.DefaultTuitionAmount,
            UnitPriceSession = request.UnitPriceSession,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-043: Xóa mềm Program
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteProgram(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProgramCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-044: Kích hoạt/Vô hiệu hóa Program
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ToggleProgramStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleProgramStatusCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

