using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Invoices.CancelInvoice;
using Kidzgo.Application.Invoices.CreateInvoice;
using Kidzgo.Application.Invoices.CreatePayOSLink;
using Kidzgo.Application.Invoices.GetInvoiceById;
using Kidzgo.Application.Invoices.GetInvoices;
using Kidzgo.Application.Invoices.GetParentInvoices;
using Kidzgo.Application.Invoices.MarkInvoiceOverdue;
using Kidzgo.Application.Invoices.UpdateInvoice;
using Kidzgo.Domain.Finance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/invoices")]
[ApiController]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly ISender _mediator;

    public InvoiceController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-253: Tạo Invoice
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> CreateInvoice(
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand
        {
            BranchId = request.BranchId,
            StudentProfileId = request.StudentProfileId,
            ClassId = request.ClassId,
            Type = request.Type,
            Amount = request.Amount,
            Currency = request.Currency,
            DueDate = request.DueDate,
            Description = request.Description,
            InvoiceLines = request.InvoiceLines?.Select(il => new CreateInvoiceLineDto
            {
                ItemType = il.ItemType,
                Quantity = il.Quantity,
                UnitPrice = il.UnitPrice,
                Description = il.Description,
                SessionIds = il.SessionIds
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(i => $"/api/invoices/{i.Id}");
    }

    /// UC-254: Xem danh sách Invoices
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="studentProfileId">Filter by student profile ID</param>
    /// <param name="classId">Filter by class ID</param>
    /// <param name="status">Invoice status: Pending, Paid, Overdue, or Cancelled</param>
    /// <param name="type">Invoice type</param>
    /// <param name="searchTerm">Search by student display name</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetInvoices(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] InvoiceType? type,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetInvoicesQuery
        {
            BranchId = branchId,
            StudentProfileId = studentProfileId,
            ClassId = classId,
            Status = status,
            Type = type,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-254a: Xem danh sách Invoices của Parent
    [HttpGet("parents/{parentProfileId:guid}")]
    [Authorize(Roles = "Parent")]
    public async Task<IResult> GetParentInvoices(
        Guid parentProfileId,
        [FromQuery] InvoiceStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentInvoicesQuery
        {
            ParentProfileId = parentProfileId,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-255: Xem chi tiết Invoice
    [HttpGet("{id:guid}")]
    public async Task<IResult> GetInvoiceById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetInvoiceByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-256: Cập nhật Invoice
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> UpdateInvoice(
        Guid id,
        [FromBody] UpdateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateInvoiceCommand
        {
            Id = id,
            BranchId = request.BranchId,
            StudentProfileId = request.StudentProfileId,
            ClassId = request.ClassId,
            Type = request.Type,
            Amount = request.Amount,
            Currency = request.Currency,
            DueDate = request.DueDate,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-257: Hủy Invoice
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> CancelInvoice(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CancelInvoiceCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-260/261: Sinh PayOS payment link và QR code
    [HttpPost("{id:guid}/payos/create-link")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> CreatePayOSLink(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CreatePayOSLinkCommand
        {
            InvoiceId = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-263: Đánh dấu Invoice OVERDUE
    [HttpPatch("{id:guid}/mark-overdue")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> MarkInvoiceOverdue(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new MarkInvoiceOverdueCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

