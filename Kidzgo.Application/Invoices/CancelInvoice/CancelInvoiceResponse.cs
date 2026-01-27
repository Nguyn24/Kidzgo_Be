using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.CancelInvoice;

public sealed class CancelInvoiceResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
}

