namespace Kidzgo.Application.Abstraction.Services;

/// <summary>
/// Service để tự động cộng Stars và XP khi hoàn thành các hoạt động
/// </summary>
public interface IGamificationService
{
    /// <summary>
    /// UC-200: Cộng Stars khi hoàn thành Mission
    /// </summary>
    Task AddStarsForMissionCompletion(Guid studentProfileId, int starsAmount, Guid missionId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// UC-201: Cộng Stars khi hoàn thành Homework
    /// </summary>
    Task AddStarsForHomeworkCompletion(Guid studentProfileId, int starsAmount, Guid homeworkId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// UC-202: Cộng Stars khi điểm danh (Attendance Streak)
    /// </summary>
    Task AddStarsForAttendance(Guid studentProfileId, int starsAmount, Guid attendanceId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// UC-203: Cộng XP khi hoàn thành Mission
    /// </summary>
    Task AddXpForMissionCompletion(Guid studentProfileId, int xpAmount, Guid missionId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// UC-204: Cộng XP khi điểm danh (Attendance Streak)
    /// </summary>
    Task AddXpForAttendance(Guid studentProfileId, int xpAmount, Guid attendanceId, string? reason = null, CancellationToken cancellationToken = default);
}

