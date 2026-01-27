using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.CreateInvoice;

public sealed class CreateInvoiceResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public string Type { get; init; } = null!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = null!;
    public DateOnly? DueDate { get; init; }
    public string Status { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? IssuedAt { get; init; }
    public Guid? IssuedBy { get; init; }
}

