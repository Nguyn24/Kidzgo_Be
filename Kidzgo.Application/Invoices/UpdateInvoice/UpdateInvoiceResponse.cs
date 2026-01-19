using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.UpdateInvoice;

public sealed class UpdateInvoiceResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public InvoiceType Type { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateOnly? DueDate { get; init; }
    public InvoiceStatus Status { get; init; }
    public string? Description { get; init; }
}

