using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.MarkInvoiceOverdue;

public sealed class MarkInvoiceOverdueCommandHandler(
    IDbContext context
) : ICommandHandler<MarkInvoiceOverdueCommand, MarkInvoiceOverdueResponse>
{
    public async Task<Result<MarkInvoiceOverdueResponse>> Handle(MarkInvoiceOverdueCommand command, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure<MarkInvoiceOverdueResponse>(
                InvoiceErrors.InvoiceNotFound);
        }

        // Only mark as overdue if status is Pending
        if (invoice.Status != InvoiceStatus.Pending)
        {
            return Result.Failure<MarkInvoiceOverdueResponse>(
                InvoiceErrors.InvoiceAlreadyPaid);
        }

        invoice.Status = InvoiceStatus.Overdue;
        await context.SaveChangesAsync(cancellationToken);

        return new MarkInvoiceOverdueResponse
        {
            Id = invoice.Id,
            Status = invoice.Status.ToString()
        };
    }
}

