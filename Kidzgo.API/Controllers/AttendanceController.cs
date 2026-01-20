using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Attendance.GetSessionAttendance;
using Kidzgo.Application.Attendance.GetStudentAttendanceHistory;
using Kidzgo.Application.Attendance.MarkAttendance;
using Kidzgo.Application.Attendance.UpdateAttendance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/attendance")]
[ApiController]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly ISender _mediator;

    public AttendanceController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-099: Điểm danh học sinh
    /// </summary>
    [HttpPost("{sessionId:guid}")]
    public async Task<IResult> Mark(
        Guid sessionId,
        [FromBody] MarkAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MarkAttendanceCommand
        {
            SessionId = sessionId,
            StudentProfileId = request.StudentProfileId,
            AttendanceStatus = request.AttendanceStatus,
            Note = request.Comment,
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-100: Danh sách điểm danh của Session
    /// </summary>
    [HttpGet("{sessionId:guid}")]
    public async Task<IResult> GetSessionAttendance(Guid sessionId, CancellationToken cancellationToken)
    {
        var query = new GetSessionAttendanceQuery { SessionId = sessionId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-101: Lịch sử điểm danh học sinh
    /// </summary>
    [HttpGet("students/{studentProfileId:guid}")]
    public async Task<IResult> GetStudentHistory(
        Guid studentProfileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentAttendanceHistoryQuery
        {
            StudentProfileId = studentProfileId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-104: Cập nhật điểm danh
    /// </summary>
    [HttpPut("{sessionId:guid}/students/{studentProfileId:guid}")]
    public async Task<IResult> Update(
        Guid sessionId,
        Guid studentProfileId,
        [FromBody] UpdateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAttendanceCommand
        {
            SessionId = sessionId,
            StudentProfileId = studentProfileId,
            AttendanceStatus = request.AttendanceStatus,
            IsAdmin = User.IsInRole("Admin")
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

