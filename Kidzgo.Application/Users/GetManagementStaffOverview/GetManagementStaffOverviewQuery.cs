using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.GetManagementStaffOverview;

public sealed class GetManagementStaffOverviewQuery : IQuery<ManagementStaffOverviewResponse>
{
    public Guid? ClassId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? LeadId { get; set; }
    public Guid? EnrollmentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

