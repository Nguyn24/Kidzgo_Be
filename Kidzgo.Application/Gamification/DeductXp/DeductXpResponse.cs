namespace Kidzgo.Application.Gamification.DeductXp;

public sealed class DeductXpResponse
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public int NewXp { get; init; }
    public string NewLevel { get; init; } = null!;
    public bool LevelDown { get; init; }
}

