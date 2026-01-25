using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Invoices.GetParentInvoices;

public sealed class GetParentInvoicesByCurrentUserQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentInvoicesByCurrentUserQuery, GetParentInvoicesResponse>
{
    public async Task<Result<GetParentInvoicesResponse>> Handle(
        GetParentInvoicesByCurrentUserQuery query, 
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get parent profile for current user
        var parentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && 
                                     p.ProfileType == ProfileType.Parent && 
                                     p.IsActive && 
                                     !p.IsDeleted, cancellationToken);

        if (parentProfile == null)
        {
            return Result.Failure<GetParentInvoicesResponse>(
                InvoiceErrors.ParentNotFound);
        }

        // Get StudentId from context (token) - only get data for selected student
        var selectedStudentId = userContext.StudentId;

        if (!selectedStudentId.HasValue)
        {
            return Result.Failure<GetParentInvoicesResponse>(
                InvoiceErrors.ParentNotFound);
        }

        // Verify the student is linked to this parent
        var isLinked = await context.ParentStudentLinks
            .AsNoTracking()
            .AnyAsync(psl => psl.ParentProfileId == parentProfile.Id && 
                            psl.StudentProfileId == selectedStudentId.Value, 
                     cancellationToken);

        if (!isLinked)
        {
            return Result.Failure<GetParentInvoicesResponse>(
                InvoiceErrors.ParentNotFound);
        }

        var invoicesQuery = context.Invoices
            .Include(i => i.Branch)
            .Include(i => i.StudentProfile)
            .Include(i => i.Class)
            .Where(i => i.StudentProfileId == selectedStudentId.Value)
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

