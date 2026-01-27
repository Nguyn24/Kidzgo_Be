using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateInvoiceCommand, CreateInvoiceResponse>
{
    public async Task<Result<CreateInvoiceResponse>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateInvoiceResponse>(
                InvoiceErrors.BranchNotFound);
        }

        // Check if student profile exists
        bool studentProfileExists = await context.Profiles
            .AnyAsync(p => p.Id == command.StudentProfileId && p.ProfileType == Domain.Users.ProfileType.Student && p.IsActive, cancellationToken);

        if (!studentProfileExists)
        {
            return Result.Failure<CreateInvoiceResponse>(
                InvoiceErrors.StudentProfileNotFound);
        }

        // Check if class exists (if provided)
        if (command.ClassId.HasValue)
        {
            bool classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<CreateInvoiceResponse>(
                    InvoiceErrors.ClassNotFound);
            }
        }

        var now = DateTime.UtcNow;
        var userId = userContext.UserId;

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            StudentProfileId = command.StudentProfileId,
            ClassId = command.ClassId,
            Type = command.Type,
            Amount = command.Amount,
            Currency = command.Currency,
            DueDate = command.DueDate,
            Status = InvoiceStatus.Pending,
            Description = command.Description,
            IssuedAt = now,
            IssuedBy = userId
        };

        // Create invoice lines if provided
        if (command.InvoiceLines != null && command.InvoiceLines.Any())
        {
            foreach (var lineDto in command.InvoiceLines)
            {
                string? sessionIdsJson = null;
                if (lineDto.SessionIds != null && lineDto.SessionIds.Any())
                {
                    sessionIdsJson = JsonSerializer.Serialize(lineDto.SessionIds);
                }

                var invoiceLine = new InvoiceLine
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    ItemType = lineDto.ItemType,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    Description = lineDto.Description,
                    SessionIds = sessionIdsJson
                };

                invoice.InvoiceLines.Add(invoiceLine);
            }
        }

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateInvoiceResponse
        {
            Id = invoice.Id,
            BranchId = invoice.BranchId,
            StudentProfileId = invoice.StudentProfileId,
            ClassId = invoice.ClassId,
            Type = invoice.Type.ToString(),
            Amount = invoice.Amount,
            Currency = invoice.Currency,
            DueDate = invoice.DueDate,
            Status = invoice.Status.ToString(),
            Description = invoice.Description,
            IssuedAt = invoice.IssuedAt,
            IssuedBy = invoice.IssuedBy
        });
    }
}

