using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Classes.AssignTeacher;
using Kidzgo.Application.Classes.ChangeClassStatus;
using Kidzgo.Application.Classes.CheckClassCapacity;
using Kidzgo.Application.Classes.CreateClass;
using Kidzgo.Application.Classes.DeleteClass;
using Kidzgo.Application.Classes.GetClassById;
using Kidzgo.Application.Classes.GetClasses;
using Kidzgo.Application.Classes.UpdateClass;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/classes")]
[ApiController]
public class ClassController : ControllerBase
{
    private readonly ISender _mediator;

    public ClassController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-057: Tạo Class
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateClass(
        [FromBody] CreateClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateClassCommand
        {
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            Code = request.Code,
            Title = request.Title,
            MainTeacherId = request.MainTeacherId,
            AssistantTeacherId = request.AssistantTeacherId,
            StartDate = request.StartDate,
            EndDate = request.EndDate, // Nếu là null hoặc default, sẽ được tính tự động trong handler
            Capacity = request.Capacity,
            SchedulePattern = request.SchedulePattern
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(c => $"/api/classes/{c.Id}");
    }

    /// UC-058: Xem danh sách Classes
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="programId">Filter by program ID</param>
    /// <param name="studentId">Filter by enrolled student ID</param>
    /// <param name="status">Class status: Planned, Active, or Closed</param>
    /// <param name="searchTerm">Search by class code or title</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetClasses(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] Guid? studentId,
        [FromQuery] string? status,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Domain.Classes.ClassStatus? classStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Classes.ClassStatus>(status, true, out var parsedStatus))
        {
            classStatus = parsedStatus;
        }

        var query = new GetClassesQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            StudentId = studentId,
            Status = classStatus,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-059: Xem chi tiết Class
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetClassById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetClassByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-060: Cập nhật Class
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateClass(
        Guid id,
        [FromBody] UpdateClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClassCommand
        {
            Id = id,
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            Code = request.Code,
            Title = request.Title,
            MainTeacherId = request.MainTeacherId,
            AssistantTeacherId = request.AssistantTeacherId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = request.Capacity,
            SchedulePattern = request.SchedulePattern
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-061: Xóa mềm Class (Set status = Closed)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteClass(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteClassCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-062: Thay đổi trạng thái Class (PLANNED/ACTIVE/CLOSED)
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ChangeClassStatus(
        Guid id,
        [FromBody] ChangeClassStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeClassStatusCommand
        {
            Id = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-063/064: Gán Main Teacher và Assistant Teacher cho Class
    [HttpPatch("{id:guid}/assign-teacher")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AssignTeacher(
        Guid id,
        [FromBody] AssignTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignTeacherCommand
        {
            ClassId = id,
            MainTeacherId = request.MainTeacherId,
            AssistantTeacherId = request.AssistantTeacherId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-066: Kiểm tra capacity trước khi ghi danh
    [HttpGet("{id:guid}/capacity")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CheckClassCapacity(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new CheckClassCapacityQuery
        {
            ClassId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

