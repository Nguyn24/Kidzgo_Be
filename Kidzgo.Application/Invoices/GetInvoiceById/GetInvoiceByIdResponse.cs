using System.Text.Json;
using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.GetInvoiceById;

public sealed class GetInvoiceByIdResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassCode { get; init; }
    public string Type { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateOnly? DueDate { get; init; }
    public string Status { get; init; } = null!;
    public string? Description { get; init; }
    public string? PayosPaymentLink { get; init; }
    public string? PayosQr { get; init; }
    public DateTime? IssuedAt { get; init; }
    public Guid? IssuedBy { get; init; }
    public string? IssuedByName { get; init; }
    public List<InvoiceLineDto> InvoiceLines { get; init; } = new();
}

public sealed class InvoiceLineDto
{
    public Guid Id { get; init; }
    public string ItemType { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string? Description { get; init; }
    public List<Guid>? SessionIds { get; set; }
    // Raw JSON string used internally for post-processing (not exposed externally)
    public string? SessionIdsRaw { get; set; }
}

