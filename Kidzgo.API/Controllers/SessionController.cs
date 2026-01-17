using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Sessions.CancelSession;
using Kidzgo.Application.Sessions.CompleteSession;
using Kidzgo.Application.Sessions.CreateSession;
using Kidzgo.Application.Sessions.GetSessionById;
using Kidzgo.Application.Sessions.GetSessions;
using Kidzgo.Application.Sessions.UpdateSession;
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
}

