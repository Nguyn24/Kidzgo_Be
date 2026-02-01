namespace Kidzgo.Application.Gamification.GetAttendanceStreak;

public sealed class GetAttendanceStreakResponse
{
    public Guid StudentProfileId { get; init; }
    public int CurrentStreak { get; init; }
    public int MaxStreak { get; init; }
    public DateOnly? LastAttendanceDate { get; init; }
    public List<AttendanceStreakDto> RecentStreaks { get; init; } = new();
}

public sealed class AttendanceStreakDto
{
    public Guid Id { get; init; }
    public DateOnly AttendanceDate { get; init; }
    public int CurrentStreak { get; init; }
    public int RewardStars { get; init; }
    public int RewardExp { get; init; }
    public DateTime CreatedAt { get; init; }
}

