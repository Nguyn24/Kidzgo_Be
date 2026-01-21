using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Services;

/// Implementation của ISchedulePatternParser sử dụng iCal.Net để parse RRULE
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

            // Dùng timezone VN (UTC+7). Trên Windows: "SE Asia Standard Time"
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            // Tạo startDateTime ở timezone VN, rồi convert sang UTC
            var startTimeOnly = TimeOnly.MinValue;
            if (recurrencePattern.ByHour.Count > 0 && recurrencePattern.ByMinute.Count > 0)
            {
                var hour = recurrencePattern.ByHour.First();
                var minute = recurrencePattern.ByMinute.First();
                startTimeOnly = new TimeOnly(hour, minute);
            }

            var startLocal = DateTime.SpecifyKind(
                startDate.ToDateTime(startTimeOnly),
                DateTimeKind.Unspecified);
            var startDateTime = TimeZoneInfo.ConvertTimeToUtc(startLocal, vnTimeZone);

            // Tạo một CalendarEvent với RRULE
            var calendarEvent = new CalendarEvent
            {
                Start = new CalDateTime(startDateTime),
                RecurrenceRules = new List<RecurrencePattern>
                {
                    recurrencePattern
                }
            };

            // Set end date nếu có (convert VN -> UTC)
            if (endDate.HasValue)
            {
                var endLocal = DateTime.SpecifyKind(
                    endDate.Value.ToDateTime(TimeOnly.MaxValue),
                    DateTimeKind.Unspecified);
                var endDateTime = TimeZoneInfo.ConvertTimeToUtc(endLocal, vnTimeZone);
                calendarEvent.RecurrenceRules[0].Until = endDateTime;
            }

            // Generate occurrences
            var occurrences = calendarEvent.GetOccurrences(
                startDateTime,
                endDate.HasValue
                    ? TimeZoneInfo.ConvertTimeToUtc(
                        DateTime.SpecifyKind(
                            endDate.Value.ToDateTime(TimeOnly.MaxValue),
                            DateTimeKind.Unspecified),
                        vnTimeZone)
                    : DateTime.MaxValue
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

