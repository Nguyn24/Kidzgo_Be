namespace Kidzgo.Application.Gamification.CheckInAttendanceStreak;

public sealed class CheckInAttendanceStreakResponse
{
    public Guid StudentProfileId { get; init; }
    public DateOnly AttendanceDate { get; init; }
    public int CurrentStreak { get; init; }
    public int MaxStreak { get; init; }
    public int RewardStars { get; init; }
    public int RewardExp { get; init; }
    public bool IsNewStreak { get; init; } // True if this is a new streak record, False if already checked in today
}

