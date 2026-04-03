using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Payments.GetParentPayments;

public sealed class GetParentPaymentsQuery : IQuery<GetParentPaymentsResponse>, IPageableQuery
{
    public Guid? StudentProfileId { get; init; }
    public Guid? InvoiceId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
