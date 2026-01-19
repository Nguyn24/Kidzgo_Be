using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.GetParentInvoices;

public sealed class GetParentInvoicesQueryHandler(
    IDbContext context
) : IQueryHandler<GetParentInvoicesQuery, GetParentInvoicesResponse>
{
    public async Task<Result<GetParentInvoicesResponse>> Handle(GetParentInvoicesQuery query, CancellationToken cancellationToken)
    {
        // Verify parent profile exists
        var parentProfile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == query.ParentProfileId && p.ProfileType == Domain.Users.ProfileType.Parent && p.IsActive, cancellationToken);

        if (parentProfile == null)
        {
            return Result.Failure<GetParentInvoicesResponse>(
                InvoiceErrors.ParentNotFound);
        }

        // Get linked student profiles
        var studentProfileIds = await context.ParentStudentLinks
            .Where(psl => psl.ParentProfileId == query.ParentProfileId)
            .Select(psl => psl.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (!studentProfileIds.Any())
        {
            return Result.Success(new GetParentInvoicesResponse
            {
                Invoices = new Page<InvoiceDto>(
                    new List<InvoiceDto>(),
                    0,
                    query.PageNumber,
                    query.PageSize)
            });
        }

        var invoicesQuery = context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.StudentProfile)
            .Include(i => i.Class)
            .Where(i => studentProfileIds.Contains(i.StudentProfileId))
            .AsQueryable();

        // Filter by status
        if (query.Status.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.Status == query.Status.Value);
        }

        // Get total count
        int totalCount = await invoicesQuery.CountAsync(cancellationToken);

        // Apply pagination
        var invoices = await invoicesQuery
            .OrderByDescending(i => i.IssuedAt ?? DateTime.MinValue)
            .ThenByDescending(i => i.Id)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                BranchId = i.BranchId,
                BranchName = i.Branch.Name,
                StudentProfileId = i.StudentProfileId,
                StudentName = i.StudentProfile.DisplayName ?? "Unknown",
                ClassId = i.ClassId,
                ClassCode = i.Class != null ? i.Class.Code : null,
                Type = i.Type,
                Amount = i.Amount,
                Currency = i.Currency,
                DueDate = i.DueDate,
                Status = i.Status,
                Description = i.Description,
                IssuedAt = i.IssuedAt,
                PayosPaymentLink = i.PayosPaymentLink,
                PayosQr = i.PayosQr
            })
            .ToListAsync(cancellationToken);

        var page = new Page<InvoiceDto>(
            invoices,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetParentInvoicesResponse
        {
            Invoices = page
        };
    }
}

