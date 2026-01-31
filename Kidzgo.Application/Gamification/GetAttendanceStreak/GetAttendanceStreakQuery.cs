using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.GetAttendanceStreak;

public sealed class GetAttendanceStreakQuery : IQuery<GetAttendanceStreakResponse>
{
    public Guid StudentProfileId { get; init; }
}

