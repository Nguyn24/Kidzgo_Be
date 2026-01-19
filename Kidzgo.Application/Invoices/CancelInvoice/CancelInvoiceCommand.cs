using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Invoices.CancelInvoice;

public sealed class CancelInvoiceCommand : ICommand<CancelInvoiceResponse>
{
    public Guid Id { get; init; }
}

