using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Classrooms.CreateClassroom;
using Kidzgo.Application.Classrooms.DeleteClassroom;
using Kidzgo.Application.Classrooms.GetClassroomById;
using Kidzgo.Application.Classrooms.GetClassrooms;
using Kidzgo.Application.Classrooms.ToggleClassroomStatus;
using Kidzgo.Application.Classrooms.UpdateClassroom;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/classrooms")]
[ApiController]
public class ClassroomController : ControllerBase
{
    private readonly ISender _mediator;

    public ClassroomController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-051: Tạo Classroom
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> CreateClassroom(
        [FromBody] CreateClassroomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateClassroomCommand
        {
            BranchId = request.BranchId,
            Name = request.Name,
            Capacity = request.Capacity,
            Note = request.Note
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(c => $"/api/classrooms/{c.Id}");
    }

    /// UC-052: Xem danh sách Classrooms
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetClassrooms(
        [FromQuery] Guid? branchId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetClassroomsQuery
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

    /// Lấy danh sách tất cả Classrooms đang active (IsActive = true)
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IResult> GetActiveClassrooms(
        [FromQuery] Guid? branchId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetClassroomsQuery
        {
            BranchId = branchId,
            SearchTerm = searchTerm,
            IsActive = true, // Chỉ lấy classrooms active
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-053: Xem chi tiết Classroom
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetClassroomById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetClassroomByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-054: Cập nhật Classroom
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> UpdateClassroom(
        Guid id,
        [FromBody] UpdateClassroomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClassroomCommand
        {
            Id = id,
            BranchId = request.BranchId,
            Name = request.Name,
            Capacity = request.Capacity,
            Note = request.Note
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-055: Xóa Classroom
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteClassroom(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteClassroomCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-056: Kích hoạt/Vô hiệu hóa Classroom
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> ToggleClassroomStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleClassroomStatusCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

