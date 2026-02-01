using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.GetParentInvoices;

public sealed class GetParentInvoicesResponse
{
    public Page<InvoiceDto> Invoices { get; init; } = null!;
}

public sealed class InvoiceDto
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
    public DateTime? IssuedAt { get; init; }
    public string? PayosPaymentLink { get; init; }
    public string? PayosQr { get; init; }
}

