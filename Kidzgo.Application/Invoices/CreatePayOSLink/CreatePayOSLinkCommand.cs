using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Invoices.CreatePayOSLink;

public sealed class CreatePayOSLinkCommand : ICommand<CreatePayOSLinkResponse>
{
    public Guid InvoiceId { get; init; }
}

