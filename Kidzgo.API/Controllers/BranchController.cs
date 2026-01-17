using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Branches.CreateBranch;
using Kidzgo.Application.Branches.DeleteBranch;
using Kidzgo.Application.Branches.GetBranchById;
using Kidzgo.Application.Branches.GetBranches;
using Kidzgo.Application.Branches.ToggleBranchStatus;
using Kidzgo.Application.Branches.UpdateBranch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/branches")]
[ApiController]
[Authorize(Roles = "Admin")]
public class BranchController : ControllerBase
{
    private readonly ISender _mediator;

    public BranchController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-384: Tạo Branch
    /// </summary>
    [HttpPost]
    public async Task<IResult> CreateBranch(
        [FromBody] CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchCommand
        {
            Code = request.Code,
            Name = request.Name,
            Address = request.Address,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(b => $"/api/branches/{b.Id}");
    }

    /// <summary>
    /// UC-385: Xem danh sách Branches
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetBranches(CancellationToken cancellationToken)
    {
        var query = new GetBranchesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-386: Xem chi tiết Branch
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetBranchById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetBranchByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-387: Cập nhật Branch
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateBranch(
        Guid id,
        [FromBody] UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBranchCommand
        {
            Id = id,
            Code = request.Code,
            Name = request.Name,
            Address = request.Address,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-388: Xóa Branch (soft delete - set IsActive = false)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteBranch(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteBranchCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-389: Kích hoạt/Vô hiệu hóa Branch
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IResult> ToggleBranchStatus(
        Guid id,
        [FromBody] ToggleBranchStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ToggleBranchStatusCommand
        {
            Id = id,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

