using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.ReportRequests.CancelReportRequest;
using Kidzgo.Application.ReportRequests.CompleteReportRequest;
using Kidzgo.Application.ReportRequests.CreateReportRequest;
using Kidzgo.Application.ReportRequests.GetReportRequestById;
using Kidzgo.Application.ReportRequests.GetReportRequests;
using Kidzgo.Domain.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/report-requests")]
[ApiController]
[Authorize]
public sealed class ReportRequestController : ControllerBase
{
    private readonly ISender _mediator;

    public ReportRequestController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateReportRequest(
        [FromBody] CreateReportRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReportRequestCommand
        {
            ReportType = request.ReportType,
            AssignedTeacherUserId = request.AssignedTeacherUserId,
            TargetStudentProfileId = request.TargetStudentProfileId,
            TargetClassId = request.TargetClassId,
            TargetSessionId = request.TargetSessionId,
            Month = request.Month,
            Year = request.Year,
            Priority = request.Priority,
            Message = request.Message,
            DueAt = request.DueAt,
            NotificationChannel = request.NotificationChannel
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/report-requests/{r.Id}");
    }

    [HttpGet]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetReportRequests(
        [FromQuery] ReportRequestType? reportType,
        [FromQuery] ReportRequestStatus? status,
        [FromQuery] ReportRequestPriority? priority,
        [FromQuery] Guid? assignedTeacherUserId,
        [FromQuery] Guid? targetStudentProfileId,
        [FromQuery] Guid? targetClassId,
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetReportRequestsQuery
        {
            ReportType = reportType,
            Status = status,
            Priority = priority,
            AssignedTeacherUserId = assignedTeacherUserId,
            TargetStudentProfileId = targetStudentProfileId,
            TargetClassId = targetClassId,
            Month = month,
            Year = year,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetReportRequestById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReportRequestByIdQuery(id), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> CompleteReportRequest(
        Guid id,
        [FromBody] CompleteReportRequestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteReportRequestCommand
        {
            Id = id,
            LinkedSessionReportId = request.LinkedSessionReportId,
            LinkedMonthlyReportId = request.LinkedMonthlyReportId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CancelReportRequest(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelReportRequestCommand(id), cancellationToken);
        return result.MatchOk();
    }
}
