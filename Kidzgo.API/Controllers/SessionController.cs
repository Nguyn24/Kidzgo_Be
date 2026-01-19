using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Sessions.CancelSession;
using Kidzgo.Application.Sessions.CheckSessionConflicts;
using Kidzgo.Application.Sessions.CompleteSession;
using Kidzgo.Application.Sessions.CreateSession;
using Kidzgo.Application.Sessions.CreateSessionRole;
using Kidzgo.Application.Sessions.DeleteSessionRole;
using Kidzgo.Application.Sessions.GetSessionById;
using Kidzgo.Application.Sessions.GetSessionRoles;
using Kidzgo.Application.Sessions.GetSessions;
using Kidzgo.Application.Sessions.UpdateSession;
using Kidzgo.Application.Sessions.UpdateSessionRole;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/sessions")]
[ApiController]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISender _mediator;

    public SessionController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-076 (manual): Tạo Session thủ công
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> CreateSession(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var participationType = Enum.TryParse<Domain.Sessions.ParticipationType>(
            request.ParticipationType, true, out var parsedType)
            ? parsedType
            : Domain.Sessions.ParticipationType.Main;

        var command = new CreateSessionCommand
        {
            ClassId = request.ClassId,
            PlannedDatetime = request.PlannedDatetime,
            DurationMinutes = request.DurationMinutes,
            PlannedRoomId = request.PlannedRoomId,
            PlannedTeacherId = request.PlannedTeacherId,
            PlannedAssistantId = request.PlannedAssistantId,
            ParticipationType = participationType
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(s => $"/api/sessions/{s.Id}");
    }

    /// <summary>
    /// UC-077: Xem danh sách Sessions (Admin/Staff)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetSessions(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? branchId,
        [FromQuery] string? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Domain.Sessions.SessionStatus? sessionStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<Domain.Sessions.SessionStatus>(status, true, out var parsedStatus))
        {
            sessionStatus = parsedStatus;
        }

        var query = new GetSessionsQuery
        {
            ClassId = classId,
            BranchId = branchId,
            Status = sessionStatus,
            From = from,
            To = to,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-078: Xem chi tiết Session
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<IResult> GetSessionById(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionByIdQuery
        {
            SessionId = sessionId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-079: Cập nhật Session (giờ/phòng/giáo viên)
    /// </summary>
    [HttpPut("{sessionId:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> UpdateSession(
        Guid sessionId,
        [FromBody] UpdateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var participationType = Enum.TryParse<Domain.Sessions.ParticipationType>(
            request.ParticipationType, true, out var parsedType)
            ? parsedType
            : Domain.Sessions.ParticipationType.Main;

        var command = new UpdateSessionCommand
        {
            SessionId = sessionId,
            PlannedDatetime = request.PlannedDatetime,
            DurationMinutes = request.DurationMinutes,
            PlannedRoomId = request.PlannedRoomId,
            PlannedTeacherId = request.PlannedTeacherId,
            PlannedAssistantId = request.PlannedAssistantId,
            ParticipationType = participationType
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-080: Hủy Session (CANCELLED)
    /// </summary>
    [HttpPost("{sessionId:guid}/cancel")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> CancelSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var command = new CancelSessionCommand
        {
            SessionId = sessionId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-081: Đánh dấu Session hoàn thành (COMPLETED)
    /// </summary>
    [HttpPost("{sessionId:guid}/complete")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> CompleteSession(
        Guid sessionId,
        [FromBody] CompleteSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteSessionCommand
        {
            SessionId = sessionId,
            ActualDatetime = request.ActualDatetime
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-082: Kiểm tra xung đột phòng/giáo viên
    /// UC-083: Gợi ý phòng/slot khác khi xung đột
    /// </summary>
    [HttpPost("check-conflicts")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> CheckSessionConflicts(
        [FromBody] CheckSessionConflictsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new CheckSessionConflictsQuery
        {
            SessionId = request.SessionId,
            PlannedDatetime = request.PlannedDatetime,
            DurationMinutes = request.DurationMinutes,
            PlannedRoomId = request.PlannedRoomId,
            PlannedTeacherId = request.PlannedTeacherId,
            PlannedAssistantId = request.PlannedAssistantId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-085: Tạo Session Role (MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP)
    /// </summary>
    [HttpPost("{sessionId:guid}/roles")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> CreateSessionRole(
        Guid sessionId,
        [FromBody] CreateSessionRoleRequest request,
        CancellationToken cancellationToken)
    {
        var roleType = Enum.TryParse<Domain.Payroll.SessionRoleType>(
            request.RoleType, true, out var parsedType)
            ? parsedType
            : throw new ArgumentException($"Invalid role type: {request.RoleType}");

        var command = new CreateSessionRoleCommand
        {
            SessionId = sessionId,
            StaffUserId = request.StaffUserId,
            RoleType = roleType,
            PayableUnitPrice = request.PayableUnitPrice,
            PayableAllowance = request.PayableAllowance
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(sr => $"/api/sessions/{sessionId}/roles/{sr.Id}");
    }

    /// <summary>
    /// UC-086: Xem danh sách Session Roles của Session
    /// </summary>
    [HttpGet("{sessionId:guid}/roles")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetSessionRoles(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionRolesQuery
        {
            SessionId = sessionId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-087: Cập nhật Session Role
    /// UC-089: Thiết lập payable_unit_price cho Session Role
    /// UC-090: Thiết lập payable_allowance cho Session Role
    /// </summary>
    [HttpPut("roles/{sessionRoleId:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> UpdateSessionRole(
        Guid sessionRoleId,
        [FromBody] UpdateSessionRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSessionRoleCommand
        {
            SessionRoleId = sessionRoleId,
            PayableUnitPrice = request.PayableUnitPrice,
            PayableAllowance = request.PayableAllowance
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-088: Xóa Session Role
    /// </summary>
    [HttpDelete("roles/{sessionRoleId:guid}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> DeleteSessionRole(
        Guid sessionRoleId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSessionRoleCommand
        {
            SessionRoleId = sessionRoleId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

