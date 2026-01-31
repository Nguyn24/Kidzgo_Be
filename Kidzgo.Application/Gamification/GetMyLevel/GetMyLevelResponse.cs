namespace Kidzgo.Application.Gamification.GetMyLevel;

public sealed class GetMyLevelResponse
{
    public Guid StudentProfileId { get; init; }
    public string Level { get; init; } = null!;
    public int Xp { get; init; }
    public int XpRequiredForNextLevel { get; init; }
}

