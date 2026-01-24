using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.GetParentOverview;

public sealed class GetParentOverviewQuery : IQuery<ParentOverviewResponse>
{
    public Guid? StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SessionId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

