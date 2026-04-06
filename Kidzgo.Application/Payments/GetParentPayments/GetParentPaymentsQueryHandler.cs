using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Payments.GetParentPayments;

public sealed class GetParentPaymentsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentPaymentsQuery, GetParentPaymentsResponse>
{
    public async Task<Result<GetParentPaymentsResponse>> Handle(
        GetParentPaymentsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var parentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId &&
                                      p.ProfileType == ProfileType.Parent &&
                                      p.IsActive &&
                                      !p.IsDeleted, cancellationToken);

        if (parentProfile is null)
        {
            return Result.Failure<GetParentPaymentsResponse>(InvoiceErrors.ParentNotFound);
        }

        var selectedStudentId = query.StudentProfileId ?? userContext.StudentId;
        if (!selectedStudentId.HasValue)
        {
            return Result.Failure<GetParentPaymentsResponse>(InvoiceErrors.ParentNotFound);
        }

        var isLinked = await context.ParentStudentLinks
            .AsNoTracking()
            .AnyAsync(psl => psl.ParentProfileId == parentProfile.Id &&
                             psl.StudentProfileId == selectedStudentId.Value, cancellationToken);

        if (!isLinked)
        {
            return Result.Failure<GetParentPaymentsResponse>(InvoiceErrors.ParentNotFound);
        }

        var paymentsQuery = context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.StudentProfileId == selectedStudentId.Value);

        if (query.InvoiceId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.InvoiceId == query.InvoiceId.Value);
        }

        var totalCount = await paymentsQuery.CountAsync(cancellationToken);

        var payments = await paymentsQuery
            .OrderByDescending(p => p.PaidAt)
            .ThenByDescending(p => p.Id)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(p => new ParentPaymentDto
            {
                Id = p.Id,
                InvoiceId = p.InvoiceId,
                Amount = p.Amount,
                Method = p.Method.ToString(),
                PaidAt = p.PaidAt,
                Status = p.PaidAt.HasValue ? "Paid" : "Pending",
                TransactionRef = p.ReferenceCode
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetParentPaymentsResponse
        {
            Payments = new Page<ParentPaymentDto>(payments, totalCount, query.PageNumber, query.PageSize)
        });
    }
}
