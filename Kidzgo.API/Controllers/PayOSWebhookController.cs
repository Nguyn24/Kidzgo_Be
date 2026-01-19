using Kidzgo.API.Extensions;
using Kidzgo.Application.Abstraction.Payments;
using Kidzgo.Application.Invoices.ProcessPayOSWebhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("webhooks/payos")]
[ApiController]
public class PayOSWebhookController : ControllerBase
{
    private readonly ISender _mediator;

    public PayOSWebhookController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-264: Webhook từ PayOS khi thanh toán thành công
    /// </summary>
    [HttpPost]
    public async Task<IResult> ProcessWebhook(
        [FromBody] PayOSWebhookRequest request,
        CancellationToken cancellationToken)
    {
        // Get signature from header
        var signature = Request.Headers["x-payos-signature"].FirstOrDefault() ?? string.Empty;

        var command = new ProcessPayOSWebhookCommand
        {
            WebhookRequest = request,
            Signature = signature
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

