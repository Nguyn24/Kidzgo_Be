using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.GetAdminOverview;

public sealed class GetAdminOverviewQuery : IQuery<AdminOverviewResponse>
{
    public Guid? BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? ProgramId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

