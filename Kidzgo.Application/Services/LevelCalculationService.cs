namespace Kidzgo.Application.Services;

/// <summary>
/// Service để tính Level từ XP
/// Công thức: Level = floor(sqrt(XP / 100)) + 1
/// Ví dụ: 
/// - 0-99 XP = Level 1
/// - 100-399 XP = Level 2
/// - 400-899 XP = Level 3
/// - 900-1599 XP = Level 4
/// - ...
/// </summary>
public sealed class LevelCalculationService : Kidzgo.Application.Abstraction.Services.ILevelCalculationService
{
    public string CalculateLevel(int totalXp)
    {
        if (totalXp < 0)
            totalXp = 0;

        // Công thức: Level = floor(sqrt(XP / 100)) + 1
        var level = (int)Math.Floor(Math.Sqrt(totalXp / 100.0)) + 1;
        return $"Level {level}";
    }

    public int GetXpRequiredForNextLevel(string currentLevel, int currentXp)
    {
        if (string.IsNullOrWhiteSpace(currentLevel))
            return 100; // Level 1 cần 100 XP

        // Parse level number từ string "Level X"
        if (!int.TryParse(currentLevel.Replace("Level", "").Trim(), out var currentLevelNum))
            return 100;

        // XP cần để lên level tiếp theo
        var nextLevelXp = (int)Math.Pow(currentLevelNum, 2) * 100;
        return Math.Max(0, nextLevelXp - currentXp);
    }
}

