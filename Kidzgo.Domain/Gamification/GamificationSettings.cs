namespace Kidzgo.Domain.Gamification;

public class GamificationSettings
{
    public int Id { get; set; }
    public int CheckInRewardStars { get; set; } = 1;
    public int CheckInRewardExp { get; set; } = 5;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
