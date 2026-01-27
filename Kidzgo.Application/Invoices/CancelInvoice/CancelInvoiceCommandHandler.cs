using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.CancelInvoice;

public sealed class CancelInvoiceCommandHandler(
    IDbContext context
) : ICommandHandler<CancelInvoiceCommand, CancelInvoiceResponse>
{
    public async Task<Result<CancelInvoiceResponse>> Handle(CancelInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure<CancelInvoiceResponse>(
                InvoiceErrors.InvoiceNotFound);
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result.Failure<CancelInvoiceResponse>(
                InvoiceErrors.InvoiceAlreadyCancelled);
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result.Failure<CancelInvoiceResponse>(
                InvoiceErrors.CannotCancelPaidInvoice);
        }

        invoice.Status = InvoiceStatus.Cancelled;
        await context.SaveChangesAsync(cancellationToken);

        return new CancelInvoiceResponse
        {
            Id = invoice.Id,
            Status = invoice.Status.ToString()
        };
    }
}

