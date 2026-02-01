using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.MarkInvoiceOverdue;

public sealed class MarkInvoiceOverdueResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
}

