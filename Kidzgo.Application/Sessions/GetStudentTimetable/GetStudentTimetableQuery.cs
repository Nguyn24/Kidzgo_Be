using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetStudentTimetable;

public sealed class GetStudentTimetableQuery : IQuery<GetStudentTimetableResponse>
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

