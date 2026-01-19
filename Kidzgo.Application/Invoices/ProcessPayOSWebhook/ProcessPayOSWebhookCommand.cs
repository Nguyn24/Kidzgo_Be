using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Payments;

namespace Kidzgo.Application.Invoices.ProcessPayOSWebhook;

public sealed class ProcessPayOSWebhookCommand : ICommand<ProcessPayOSWebhookResponse>
{
    public PayOSWebhookRequest WebhookRequest { get; init; } = null!;
    public string Signature { get; init; } = null!;
}

