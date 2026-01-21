using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Abstraction.Services;

/// <summary>
/// Service để parse schedule pattern (RRULE) và generate các datetime occurrences
/// </summary>
public interface ISchedulePatternParser
{
    /// <summary>
    /// Parse RRULE pattern và generate các datetime occurrences trong khoảng startDate đến endDate
    /// </summary>
    /// <param name="rrulePattern">RRULE pattern (ví dụ: "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0")</param>
    /// <param name="startDate">Ngày bắt đầu (inclusive)</param>
    /// <param name="endDate">Ngày kết thúc (inclusive)</param>
    /// <returns>Danh sách các datetime occurrences (UTC)</returns>
    Result<List<DateTime>> ParseAndGenerateOccurrences(string rrulePattern, DateOnly startDate, DateOnly? endDate);

    /// <summary>
    /// Parse duration (phút) từ RRULE pattern. Nếu không có DURATION trong pattern, trả về null.
    /// </summary>
    /// <param name="rrulePattern">RRULE pattern có thể chứa DURATION (ví dụ: "RRULE:FREQ=WEEKLY;BYDAY=MO;BYHOUR=18;BYMINUTE=0;DURATION=90")</param>
    /// <returns>Duration tính bằng phút, hoặc null nếu không có trong pattern</returns>
    int? ParseDuration(string rrulePattern);
}

