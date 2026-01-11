using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Tickets.AddTicketComment;
using Kidzgo.Application.Tickets.AssignTicket;
using Kidzgo.Application.Tickets.CreateTicket;
using Kidzgo.Application.Tickets.GetTicketById;
using Kidzgo.Application.Tickets.GetTicketHistory;
using Kidzgo.Application.Tickets.GetTicketSLA;
using Kidzgo.Application.Tickets.GetTickets;
using Kidzgo.Application.Tickets.UpdateTicketStatus;
using Kidzgo.Domain.Tickets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/tickets")]
[ApiController]
public class TicketController : ControllerBase
{
    private readonly ISender _mediator;
    public TicketController(ISender mediator)
    {
        _mediator = mediator;
    }
    /// <summary>
    /// UC-340: Parent/Student tạo Ticket
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IResult> CreateTicket(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTicketCommand
        {
            OpenedByProfileId = request.OpenedByProfileId,
            BranchId = request.BranchId,
            ClassId = request.ClassId,
            Category = request.Category,
            Subject = request.Subject,
            Message = request.Message
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(t => $"/api/tickets/{t.Id}");
    }

    /// <summary>
    /// UC-341: Xem danh sách Tickets
    /// </summary>
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="openedByUserId">Filter by opened by user ID</param>
    /// <param name="assignedToUserId">Filter by assigned to user ID</param>
    /// <param name="status">Ticket status: Open, InProgress, Resolved, or Closed</param>
    /// <param name="category">Ticket category: Homework, Finance, Schedule, or Tech</param>
    /// <param name="classId">Filter by class ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetTickets(
        [FromQuery] bool? mine,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? openedByUserId,
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] string? status,
        [FromQuery] string? category,
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        TicketStatus? ticketStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
        {
            ticketStatus = parsedStatus;
        }

        TicketCategory? ticketCategory = null;
        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<TicketCategory>(category, true, out var parsedCategory))
        {
            ticketCategory = parsedCategory;
        }

        var query = new GetTicketsQuery
        {
            Mine = mine,
            BranchId = branchId,
            OpenedByUserId = openedByUserId,
            AssignedToUserId = assignedToUserId,
            Status = ticketStatus,
            Category = ticketCategory,
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-342: Xem chi tiết Ticket
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IResult> GetTicketById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTicketByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-343: Gán Ticket cho Staff/Teacher
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> AssignTicket(
        Guid id,
        [FromBody] AssignTicketRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignTicketCommand
        {
            Id = id,
            AssignedToUserId = request.AssignedToUserId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-344: Cập nhật trạng thái Ticket (OPEN/IN_PROGRESS/RESOLVED/CLOSED)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    public async Task<IResult> UpdateTicketStatus(
        Guid id,
        [FromBody] UpdateTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTicketStatusCommand
        {
            Id = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-345: Thêm comment vào Ticket
    /// </summary>
    [HttpPost("{id:guid}/comments")]
    [Authorize]
    public async Task<IResult> AddTicketComment(
        Guid id,
        [FromBody] AddTicketCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddTicketCommentCommand
        {
            TicketId = id,
            CommenterProfileId = request.CommenterProfileId,
            Message = request.Message,
            AttachmentUrl = request.AttachmentUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(c => $"/api/tickets/{id}/comments/{c.Id}");
    }

    /// <summary>
    /// UC-347: Xem lịch sử Ticket
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [Authorize]
    public async Task<IResult> GetTicketHistory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTicketHistoryQuery
        {
            TicketId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-348: Theo dõi SLA phản hồi Ticket
    /// </summary>
    [HttpGet("{id:guid}/sla")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetTicketSLA(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTicketSLAQuery
        {
            TicketId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}