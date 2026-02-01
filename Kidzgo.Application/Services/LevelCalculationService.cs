namespace Kidzgo.Application.Services;

/// <summary>
/// Service để tính Level từ XP
/// Công thức: Level = floor(XP / 100) + 1
/// Mỗi level cần 100 XP cố định
/// Ví dụ: 
/// - 0-99 XP = Level 1
/// - 100-199 XP = Level 2
/// - 200-299 XP = Level 3
/// - 300-399 XP = Level 4
/// - ...
/// </summary>
public sealed class LevelCalculationService : Kidzgo.Application.Abstraction.Services.ILevelCalculationService
{
    public string CalculateLevel(int totalXp)
    {
        if (totalXp < 0)
            totalXp = 0;

        // Công thức: Level = floor(XP / 100) + 1
        // Mỗi level cần 100 XP cố định
        var level = (totalXp / 100) + 1;
        return $"Level {level}";
    }

    public int GetXpRequiredForNextLevel(string currentLevel, int currentXp)
    {
        // Mỗi level cần 100 XP cố định
        // XP cần = 100 - (currentXp % 100)
        // Nếu currentXp % 100 == 0, nghĩa là đã đủ XP cho level hiện tại, cần 100 XP nữa
        var remainder = currentXp % 100;
        return remainder == 0 ? 100 : 100 - remainder;
    }
}

