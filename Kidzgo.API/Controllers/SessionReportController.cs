using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.SessionReports.CreateSessionReport;
using Kidzgo.Application.SessionReports.GetSessionReportById;
using Kidzgo.Application.SessionReports.GetSessionReports;
using Kidzgo.Application.SessionReports.GetTeacherSessionReports;
using Kidzgo.Application.SessionReports.UpdateSessionReport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/session-reports")]
[ApiController]
[Authorize]
public class SessionReportController : ControllerBase
{
    private readonly ISender _mediator;

    public SessionReportController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-163: Teacher tạo Session Report
    /// UC-164: Teacher ghi feedback cho từng học sinh
    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> CreateSessionReport(
        [FromBody] CreateSessionReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSessionReportCommand
        {
            SessionId = request.SessionId,
            StudentProfileId = request.StudentProfileId,
            ReportDate = request.ReportDate,
            Feedback = request.Feedback
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(sr => $"/api/session-reports/{sr.Id}");
    }

    /// UC-165: Xem danh sách Session Reports
    /// UC-168: Filter Session Reports theo date range
    [HttpGet]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetSessionReports(
        [FromQuery] Guid? sessionId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? teacherUserId,
        [FromQuery] Guid? classId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSessionReportsQuery
        {
            SessionId = sessionId,
            StudentProfileId = studentProfileId,
            TeacherUserId = teacherUserId,
            ClassId = classId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-166: Xem chi tiết Session Report
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetSessionReportById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionReportByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-167: Cập nhật Session Report
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> UpdateSessionReport(
        Guid id,
        [FromBody] UpdateSessionReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSessionReportCommand
        {
            Id = id,
            Feedback = request.Feedback
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-169: Xem Session Reports của giáo viên trong tháng
    [HttpGet("teachers/{teacherUserId:guid}/monthly")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetTeacherSessionReports(
        Guid teacherUserId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherSessionReportsQuery
        {
            TeacherUserId = teacherUserId,
            Year = year,
            Month = month,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

