using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Invoices.GetInvoiceById;

public sealed class GetInvoiceByIdQuery : IQuery<GetInvoiceByIdResponse>
{
    public Guid Id { get; init; }
}

