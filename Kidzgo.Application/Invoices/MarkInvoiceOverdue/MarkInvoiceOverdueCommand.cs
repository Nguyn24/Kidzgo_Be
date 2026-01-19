using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Invoices.MarkInvoiceOverdue;

public sealed class MarkInvoiceOverdueCommand : ICommand<MarkInvoiceOverdueResponse>
{
    public Guid Id { get; init; }
}

