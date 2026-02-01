namespace Kidzgo.Application.Abstraction.Services;

/// <summary>
/// Service để tính Level từ XP
/// </summary>
public interface ILevelCalculationService
{
    /// <summary>
    /// Tính level từ tổng XP
    /// </summary>
    /// <param name="totalXp">Tổng XP của học sinh</param>
    /// <returns>Level hiện tại (ví dụ: "Level 1", "Level 2", ...)</returns>
    string CalculateLevel(int totalXp);

    /// <summary>
    /// Tính XP cần thiết để lên level tiếp theo
    /// </summary>
    /// <param name="currentLevel">Level hiện tại (ví dụ: "Level 1")</param>
    /// <param name="currentXp">XP hiện tại</param>
    /// <returns>XP cần thiết để lên level tiếp theo</returns>
    int GetXpRequiredForNextLevel(string currentLevel, int currentXp);
}

