using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetTeacherTimetable;

public sealed class GetTeacherTimetableQuery : IQuery<GetTeacherTimetableResponse>
{
    public Guid? TeacherUserId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ClassId { get; init; }
}

