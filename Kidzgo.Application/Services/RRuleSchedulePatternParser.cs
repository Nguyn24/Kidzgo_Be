using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Services;

/// <summary>
/// Implementation của ISchedulePatternParser sử dụng iCal.Net để parse RRULE
/// </summary>
public sealed class RRuleSchedulePatternParser : ISchedulePatternParser
{
    public Result<List<DateTime>> ParseAndGenerateOccurrences(string rrulePattern, DateOnly startDate, DateOnly? endDate)
    {
        if (string.IsNullOrWhiteSpace(rrulePattern))
        {
            return Result.Failure<List<DateTime>>(
                Error.Validation("SchedulePattern.Empty", "Schedule pattern cannot be empty"));
        }

        try
        {
            // Parse RRULE pattern
            // RRULE pattern có thể có prefix "RRULE:" hoặc không
            var pattern = rrulePattern.Trim();
            if (!pattern.StartsWith("RRULE:", StringComparison.OrdinalIgnoreCase))
            {
                pattern = "RRULE:" + pattern;
            }

            // Parse BYHOUR và BYMINUTE từ pattern để set Start time đúng
            var recurrencePattern = new RecurrencePattern(pattern);
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            
            // Nếu có BYHOUR và BYMINUTE trong pattern, sử dụng chúng
            if (recurrencePattern.ByHour.Count > 0 && recurrencePattern.ByMinute.Count > 0)
            {
                var hour = recurrencePattern.ByHour.First();
                var minute = recurrencePattern.ByMinute.First();
                startDateTime = startDate.ToDateTime(new TimeOnly(hour, minute), DateTimeKind.Utc);
            }

            // Tạo một CalendarEvent với RRULE
            var calendarEvent = new CalendarEvent
            {
                Start = new CalDateTime(startDateTime),
                RecurrenceRules = new List<RecurrencePattern>
                {
                    recurrencePattern
                }
            };

            // Set end date nếu có
            if (endDate.HasValue)
            {
                var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
                calendarEvent.RecurrenceRules[0].Until = endDateTime;
            }

            // Generate occurrences
            var occurrences = calendarEvent.GetOccurrences(
                startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                endDate?.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc) ?? DateTime.MaxValue
            ).ToList();

            // Convert sang UTC DateTime list và filter chỉ lấy trong khoảng startDate đến endDate
            var result = occurrences
                .Where(occ => occ.Period.StartTime.Value >= startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc))
                .Where(occ => !endDate.HasValue ||
                              DateOnly.FromDateTime(occ.Period.StartTime.Value.Date) <= endDate.Value)
                .Select(occ => occ.Period.StartTime.Value.ToUniversalTime())
                .OrderBy(dt => dt)
                .ToList();

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<DateTime>>(
                Error.Validation("SchedulePattern.Invalid", $"Invalid RRULE pattern: {ex.Message}"));
        }
    }
}

