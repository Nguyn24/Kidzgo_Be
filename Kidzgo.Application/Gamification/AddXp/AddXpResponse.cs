namespace Kidzgo.Application.Gamification.AddXp;

public sealed class AddXpResponse
{
    public Guid StudentProfileId { get; init; }
    public int Amount { get; init; }
    public int NewXp { get; init; }
    public string NewLevel { get; init; } = null!;
    public bool LevelUp { get; init; }
}

