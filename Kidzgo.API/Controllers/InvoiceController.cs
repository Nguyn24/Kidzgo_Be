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

    /// <summary>
    /// UC-253: Tạo Invoice
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
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

    /// <summary>
    /// UC-254: Xem danh sách Invoices
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetInvoices(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        InvoiceStatus? invoiceStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
        {
            invoiceStatus = parsedStatus;
        }

        InvoiceType? invoiceType = null;
        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<InvoiceType>(type, true, out var parsedType))
        {
            invoiceType = parsedType;
        }

        var query = new GetInvoicesQuery
        {
            BranchId = branchId,
            StudentProfileId = studentProfileId,
            ClassId = classId,
            Status = invoiceStatus,
            Type = invoiceType,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-254a: Xem danh sách Invoices của Parent
    /// </summary>
    [HttpGet("parents/{parentProfileId:guid}")]
    [Authorize(Roles = "Parent")]
    public async Task<IResult> GetParentInvoices(
        Guid parentProfileId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        InvoiceStatus? invoiceStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var parsedStatus))
        {
            invoiceStatus = parsedStatus;
        }

        var query = new GetParentInvoicesQuery
        {
            ParentProfileId = parentProfileId,
            Status = invoiceStatus,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-255: Xem chi tiết Invoice
    /// </summary>
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

    /// <summary>
    /// UC-256: Cập nhật Invoice
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
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

    /// <summary>
    /// UC-257: Hủy Invoice
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Staff")]
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

    /// <summary>
    /// UC-260/261: Sinh PayOS payment link và QR code
    /// </summary>
    [HttpPost("{id:guid}/payos/create-link")]
    [Authorize(Roles = "Admin,Staff")]
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

    /// <summary>
    /// UC-263: Đánh dấu Invoice OVERDUE
    /// </summary>
    [HttpPatch("{id:guid}/mark-overdue")]
    [Authorize(Roles = "Admin,Staff")]
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

