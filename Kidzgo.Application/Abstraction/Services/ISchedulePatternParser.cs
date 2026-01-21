using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Abstraction.Services;

/// Service để parse schedule pattern (RRULE) và generate các datetime occurrences
public interface ISchedulePatternParser
{
    /// Parse RRULE pattern và generate các datetime occurrences trong khoảng startDate đến endDate
    /// <param name="rrulePattern">RRULE pattern (ví dụ: "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0")</param>
    /// <param name="startDate">Ngày bắt đầu (inclusive)</param>
    /// <param name="endDate">Ngày kết thúc (inclusive)</param>
    /// <returns>Danh sách các datetime occurrences (UTC)</returns>
    Result<List<DateTime>> ParseAndGenerateOccurrences(string rrulePattern, DateOnly startDate, DateOnly? endDate);
}