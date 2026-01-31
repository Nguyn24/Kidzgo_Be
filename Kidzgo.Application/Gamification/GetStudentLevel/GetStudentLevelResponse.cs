namespace Kidzgo.Application.Gamification.GetStudentLevel;

public sealed class GetStudentLevelResponse
{
    public Guid StudentProfileId { get; init; }
    public string Level { get; init; } = null!;
    public int Xp { get; init; }
    public int XpRequiredForNextLevel { get; init; }
}

