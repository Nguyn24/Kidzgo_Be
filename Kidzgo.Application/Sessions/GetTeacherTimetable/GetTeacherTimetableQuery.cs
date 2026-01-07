using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetTeacherTimetable;

public sealed class GetTeacherTimetableQuery : IQuery<GetTeacherTimetableResponse>
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

