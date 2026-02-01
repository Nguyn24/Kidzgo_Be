using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.GetAccountantStaffOverview;

public sealed class GetAccountantStaffOverviewQuery : IQuery<AccountantStaffOverviewResponse>
{
    public Guid? StudentProfileId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

