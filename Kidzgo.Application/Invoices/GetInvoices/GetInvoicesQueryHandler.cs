using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.GetInvoices;

public sealed class GetInvoicesQueryHandler(
    IDbContext context
) : IQueryHandler<GetInvoicesQuery, GetInvoicesResponse>
{
    public async Task<Result<GetInvoicesResponse>> Handle(GetInvoicesQuery query, CancellationToken cancellationToken)
    {
        var invoicesQuery = context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.StudentProfile)
            .Include(i => i.Class)
            .Include(i => i.IssuedByUser)
            .AsQueryable();

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.BranchId == query.BranchId.Value);
        }

        // Filter by student profile
        if (query.StudentProfileId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.StudentProfileId == query.StudentProfileId.Value);
        }

        // Filter by class
        if (query.ClassId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.ClassId == query.ClassId.Value);
        }

        // Filter by status
        if (query.Status.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.Status == query.Status.Value);
        }

        // Filter by type
        if (query.Type.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.Type == query.Type.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            invoicesQuery = invoicesQuery.Where(i =>
                (i.Description != null && i.Description.Contains(query.SearchTerm)) ||
                (i.StudentProfile.DisplayName != null && i.StudentProfile.DisplayName.Contains(query.SearchTerm)));
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
                IssuedBy = i.IssuedBy,
                IssuedByName = i.IssuedByUser != null ? i.IssuedByUser.Name : null
            })
            .ToListAsync(cancellationToken);

        var page = new Page<InvoiceDto>(
            invoices,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetInvoicesResponse
        {
            Invoices = page
        };
    }
}

