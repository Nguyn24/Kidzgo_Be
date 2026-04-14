using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.IncidentReports.AddIncidentReportComment;
using Kidzgo.Application.IncidentReports.AssignIncidentReport;
using Kidzgo.Application.IncidentReports.CreateIncidentReport;
using Kidzgo.Application.IncidentReports.GetIncidentReportById;
using Kidzgo.Application.IncidentReports.GetIncidentReports;
using Kidzgo.Application.IncidentReports.GetIncidentReportStatistics;
using Kidzgo.Application.IncidentReports.UpdateIncidentReportStatus;
using Kidzgo.Domain.Tickets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/incident-reports")]
[ApiController]
[Authorize(Roles = "Teacher,ManagementStaff,AccountantStaff,Admin")]
public sealed class IncidentReportController : ControllerBase
{
    private readonly ISender _mediator;

    public IncidentReportController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreateIncidentReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateIncidentReportCommand
        {
            BranchId = request.BranchId,
            ClassId = request.ClassId,
            Category = request.Category,
            Subject = request.Subject,
            Message = request.Message,
            EvidenceUrl = request.EvidenceUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(x => $"/api/incident-reports/{x.Id}");
    }

    [HttpGet]
    public async Task<IResult> GetList(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? openedByUserId,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] Guid? classId,
        [FromQuery] IncidentReportCategory? category,
        [FromQuery] IncidentReportStatus? status,
        [FromQuery] string? keyword,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetIncidentReportsQuery
        {
            BranchId = branchId,
            OpenedByUserId = openedByUserId,
            AssignedToUserId = assignedToUserId,
            ClassId = classId,
            Category = category,
            Status = status,
            Keyword = keyword,
            CreatedFrom = createdFrom,
            CreatedTo = createdTo,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIncidentReportByIdQuery(id), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<IResult> AddComment(
        Guid id,
        [FromBody] AddIncidentReportCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddIncidentReportCommentCommand
        {
            TicketId = id,
            Message = request.Message,
            AttachmentUrl = request.AttachmentUrl,
            CommentType = request.CommentType
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(x => $"/api/incident-reports/{id}/comments/{x.Id}");
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Assign(
        Guid id,
        [FromBody] AssignIncidentReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignIncidentReportCommand
        {
            Id = id,
            AssignedToUserId = request.AssignedToUserId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateIncidentReportStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateIncidentReportStatusCommand
        {
            Id = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetStatistics(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? openedByUserId,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] Guid? classId,
        [FromQuery] IncidentReportCategory? category,
        [FromQuery] IncidentReportStatus? status,
        [FromQuery] string? keyword,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        CancellationToken cancellationToken = default)
    {
        var query = new GetIncidentReportStatisticsQuery
        {
            BranchId = branchId,
            OpenedByUserId = openedByUserId,
            AssignedToUserId = assignedToUserId,
            ClassId = classId,
            Category = category,
            Status = status,
            Keyword = keyword,
            CreatedFrom = createdFrom,
            CreatedTo = createdTo
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
