using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Payments;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.ProcessPayOSWebhook;

public sealed class ProcessPayOSWebhookCommandHandler(
    IDbContext context,
    IPayOSService payOSService
) : ICommandHandler<ProcessPayOSWebhookCommand, ProcessPayOSWebhookResponse>
{
    public async Task<Result<ProcessPayOSWebhookResponse>> Handle(ProcessPayOSWebhookCommand command, CancellationToken cancellationToken)
    {
        var webhook = command.WebhookRequest;

        // Verify webhook signature
        var webhookDataJson = System.Text.Json.JsonSerializer.Serialize(webhook.Data);
        if (!payOSService.VerifyWebhookSignature(command.Signature, webhookDataJson))
        {
            return Result.Failure<ProcessPayOSWebhookResponse>(
                Error.Validation(
                    "PayOS.InvalidSignature",
                    "Invalid webhook signature"));
        }

        // Check if webhook indicates successful payment
        if (webhook.Code != 0 || webhook.Data == null)
        {
            return Result.Success(new ProcessPayOSWebhookResponse
            {
                Success = false,
                Message = webhook.Desc ?? "Payment failed"
            });
        }

        // Find invoice by PayOS order code
        var invoice = await context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.PayosOrderCode == webhook.Data.OrderCode, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure<ProcessPayOSWebhookResponse>(
                InvoiceErrors.InvoiceNotFound);
        }

        // Check if payment already processed (idempotency)
        var existingPayment = invoice.Payments
            .FirstOrDefault(p => p.ReferenceCode == webhook.Data.Reference);

        if (existingPayment != null)
        {
            return Result.Success(new ProcessPayOSWebhookResponse
            {
                Success = true,
                Message = "Payment already processed"
            });
        }

        // Create payment record
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            Method = PaymentMethod.Payos,
            Amount = webhook.Data.Amount / 1000m, // Convert from VND (đồng) back to thousands
            PaidAt = DateTime.TryParse(webhook.Data.TransactionDateTime, out var paidAt)
                ? paidAt.ToUniversalTime()
                : DateTime.UtcNow,
            ReferenceCode = webhook.Data.Reference
        };

        context.Payments.Add(payment);

        // Update invoice status to Paid
        invoice.Status = InvoiceStatus.Paid;

        // TODO: Create CashbookEntry (UC-274)
        // This should be done in a separate handler or service
        // For now, we'll just update the invoice status

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProcessPayOSWebhookResponse
        {
            Success = true,
            Message = "Payment processed successfully"
        });
    }
}

