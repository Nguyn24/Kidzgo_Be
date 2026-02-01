using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.GetTeacherOverview;

public sealed class GetTeacherOverviewQuery : IQuery<TeacherOverviewResponse>
{
    public Guid? ClassId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? SessionId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

