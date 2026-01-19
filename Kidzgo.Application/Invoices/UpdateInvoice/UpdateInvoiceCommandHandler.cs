using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.UpdateInvoice;

public sealed class UpdateInvoiceCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateInvoiceCommand, UpdateInvoiceResponse>
{
    public async Task<Result<UpdateInvoiceResponse>> Handle(UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.Id, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure<UpdateInvoiceResponse>(
                InvoiceErrors.InvoiceNotFound);
        }

        // Cannot update paid or cancelled invoices
        if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result.Failure<UpdateInvoiceResponse>(
                InvoiceErrors.CannotCancelPaidInvoice);
        }

        // Check if branch exists (if provided)
        if (command.BranchId.HasValue)
        {
            bool branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchId.Value && b.IsActive, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<UpdateInvoiceResponse>(
                    InvoiceErrors.BranchNotFound);
            }

            invoice.BranchId = command.BranchId.Value;
        }

        // Check if student profile exists (if provided)
        if (command.StudentProfileId.HasValue)
        {
            bool studentProfileExists = await context.Profiles
                .AnyAsync(p => p.Id == command.StudentProfileId.Value && p.ProfileType == Domain.Users.ProfileType.Student && p.IsActive, cancellationToken);

            if (!studentProfileExists)
            {
                return Result.Failure<UpdateInvoiceResponse>(
                    InvoiceErrors.StudentProfileNotFound);
            }

            invoice.StudentProfileId = command.StudentProfileId.Value;
        }

        // Check if class exists (if provided)
        if (command.ClassId.HasValue)
        {
            bool classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<UpdateInvoiceResponse>(
                    InvoiceErrors.ClassNotFound);
            }

            invoice.ClassId = command.ClassId;
        }

        // Update fields
        if (command.Type.HasValue)
        {
            invoice.Type = command.Type.Value;
        }

        if (command.Amount.HasValue)
        {
            invoice.Amount = command.Amount.Value;
        }

        if (!string.IsNullOrEmpty(command.Currency))
        {
            invoice.Currency = command.Currency;
        }

        if (command.DueDate.HasValue)
        {
            invoice.DueDate = command.DueDate;
        }

        if (command.Description != null)
        {
            invoice.Description = command.Description;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateInvoiceResponse
        {
            Id = invoice.Id,
            BranchId = invoice.BranchId,
            StudentProfileId = invoice.StudentProfileId,
            ClassId = invoice.ClassId,
            Type = invoice.Type,
            Amount = invoice.Amount,
            Currency = invoice.Currency,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            Description = invoice.Description
        };
    }
}

