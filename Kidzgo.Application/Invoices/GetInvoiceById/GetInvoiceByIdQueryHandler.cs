using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.GetInvoiceById;

public sealed class GetInvoiceByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetInvoiceByIdQuery, GetInvoiceByIdResponse>
{
    public async Task<Result<GetInvoiceByIdResponse>> Handle(GetInvoiceByIdQuery query, CancellationToken cancellationToken)
    {
        var invoiceRaw = await context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.StudentProfile)
            .Include(i => i.Class)
            .Include(i => i.IssuedByUser)
            .Include(i => i.InvoiceLines)
            .Where(i => i.Id == query.Id)
            .Select(i => new GetInvoiceByIdResponse
            {
                Id = i.Id,
                BranchId = i.BranchId,
                BranchName = i.Branch.Name,
                StudentProfileId = i.StudentProfileId,
                StudentName = i.StudentProfile.DisplayName ?? "Unknown",
                ClassId = i.ClassId,
                ClassCode = i.Class != null ? i.Class.Code : null,
                Type = i.Type.ToString(),
                Amount = i.Amount,
                Currency = i.Currency,
                DueDate = i.DueDate,
                Status = i.Status.ToString(),
                Description = i.Description,
                PayosPaymentLink = i.PayosPaymentLink,
                PayosQr = i.PayosQr,
                IssuedAt = i.IssuedAt,
                IssuedBy = i.IssuedBy,
                IssuedByName = i.IssuedByUser != null ? i.IssuedByUser.Name : null,
                InvoiceLines = i.InvoiceLines.Select(il => new InvoiceLineDto
                {
                    Id = il.Id,
                    ItemType = il.ItemType.ToString(),
                    Quantity = il.Quantity,
                    UnitPrice = il.UnitPrice,
                    Description = il.Description,
                    SessionIds = !string.IsNullOrEmpty(il.SessionIds) ? new List<Guid>() : null, // placeholder, will deserialize after materialization
                    // keep raw string for later deserialization
                    SessionIdsRaw = il.SessionIds
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (invoiceRaw is null)
        {
            return Result.Failure<GetInvoiceByIdResponse>(
                InvoiceErrors.InvoiceNotFound);
        }

        // Deserialize SessionIds after EF materialization (avoid expression tree restrictions)
        foreach (var line in invoiceRaw.InvoiceLines)
        {
            if (line.SessionIdsRaw != null && line.SessionIdsRaw.Length > 0)
            {
                line.SessionIds = JsonSerializer.Deserialize<List<Guid>>(line.SessionIdsRaw);
            }
            else
            {
                line.SessionIds = null;
            }
        }

        return invoiceRaw;
    }
}

