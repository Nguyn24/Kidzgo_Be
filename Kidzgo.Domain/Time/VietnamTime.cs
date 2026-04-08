using System.Globalization;
using NodaTime;

namespace Kidzgo.Domain.Time;

public static class VietnamTime
{
    public const string TimeZoneId = "Asia/Ho_Chi_Minh";

    private static readonly DateTimeZone VietnamZone = DateTimeZoneProviders.Tzdb[TimeZoneId];
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    private static readonly string[] SupportedLocalDateTimeFormats =
    [
        "dd/MM/yyyy hh:mm:ss tt",
        "d/M/yyyy hh:mm:ss tt",
        "dd/M/yyyy hh:mm:ss tt",
        "d/MM/yyyy hh:mm:ss tt",
        "yyyy-MM-dd'T'HH:mm:ss",
        "yyyy-MM-dd'T'HH:mm:ss.fff",
        "yyyy-MM-dd'T'HH:mm:ss.fffffff",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm"
    ];

    private static readonly string[] SupportedLocalDateFormats =
    [
        "yyyy-MM-dd",
        "dd/MM/yyyy",
        "d/M/yyyy",
        "dd-MM-yyyy"
    ];

    public static DateTime UtcNow() => SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

    public static DateTime NowInVietnam()
        => SystemClock.Instance.GetCurrentInstant()
            .InZone(VietnamZone)
            .LocalDateTime
            .ToDateTimeUnspecified();

    public static DateTime TodayStartUtc()
        => SystemClock.Instance.GetCurrentInstant()
            .InZone(VietnamZone)
            .Date
            .AtStartOfDayInZone(VietnamZone)
            .ToInstant()
            .ToDateTimeUtc();

    public static DateOnly TodayDateOnly() => ToVietnamDateOnly(UtcNow());

    public static DateOnly ToVietnamDateOnly(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            return DateOnly.FromDateTime(value);
        }

        return DateOnly.FromDateTime(ToVietnamDateTime(value));
    }

    public static TimeOnly ToVietnamTimeOnly(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            return TimeOnly.FromDateTime(value);
        }

        return TimeOnly.FromDateTime(ToVietnamDateTime(value));
    }

    public static DateTime ToVietnamDateTime(DateTime value)
    {
        if (value.Kind == DateTimeKind.Unspecified)
        {
            return value;
        }

        var instant = Instant.FromDateTimeUtc(NormalizeToUtc(value));
        return instant.InZone(VietnamZone).LocalDateTime.ToDateTimeUnspecified();
    }

    public static DateTime NormalizeToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            return value;
        }

        if (value.Kind == DateTimeKind.Local)
        {
            return value.ToUniversalTime();
        }

        return TreatAsVietnamLocal(value);
    }

    public static DateTime? NormalizeToUtc(DateTime? value)
        => value.HasValue ? NormalizeToUtc(value.Value) : null;

    public static DateTime EnsureUtcKind(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    public static string FormatUtcAsVietnamOffset(DateTime value)
    {
        var utcValue = EnsureUtcKind(value);
        var instant = Instant.FromDateTimeUtc(utcValue);
        var zoned = instant.InZone(VietnamZone);
        var dateTimeOffset = new DateTimeOffset(
            zoned.LocalDateTime.ToDateTimeUnspecified(),
            zoned.Offset.ToTimeSpan());

        return dateTimeOffset.ToString("yyyy-MM-dd'T'HH:mm:sszzz", InvariantCulture);
    }

    public static string FormatInVietnam(DateTime value, string format)
        => ToVietnamDateTime(value).ToString(format, InvariantCulture);

    public static bool TryParseApiDateTime(string? input, out DateTime utcValue)
    {
        utcValue = default;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (HasExplicitOffset(input) &&
            DateTimeOffset.TryParse(
                input,
                InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                out var dateTimeOffset))
        {
            utcValue = dateTimeOffset.UtcDateTime;
            return true;
        }

        if (DateTime.TryParseExact(
                input,
                SupportedLocalDateTimeFormats,
                InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var localDateTime))
        {
            utcValue = TreatAsVietnamLocal(localDateTime);
            return true;
        }

        if (DateOnly.TryParseExact(
                input,
                SupportedLocalDateFormats,
                InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var dateOnly))
        {
            utcValue = TreatAsVietnamLocal(dateOnly.ToDateTime(TimeOnly.MinValue));
            return true;
        }

        if (DateTime.TryParse(
                input,
                InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var fallback))
        {
            utcValue = fallback.Kind == DateTimeKind.Unspecified
                ? TreatAsVietnamLocal(fallback)
                : NormalizeToUtc(fallback);
            return true;
        }

        return false;
    }

    public static DateTime TreatAsVietnamLocal(DateTime value)
    {
        var unspecified = DateTime.SpecifyKind(value, DateTimeKind.Unspecified);
        var localDateTime = LocalDateTime.FromDateTime(unspecified);
        return localDateTime.InZoneLeniently(VietnamZone).ToInstant().ToDateTimeUtc();
    }

    public static DateTime EndOfVietnamDayUtc(DateTime value)
    {
        var instant = Instant.FromDateTimeUtc(EnsureUtcKind(value));
        var localDate = instant.InZone(VietnamZone).Date;
        var nextDayStart = localDate.PlusDays(1).AtStartOfDayInZone(VietnamZone).ToInstant();
        return nextDayStart.Minus(Duration.FromTicks(1)).ToDateTimeUtc();
    }

    private static bool HasExplicitOffset(string input)
    {
        if (input.EndsWith('Z') || input.EndsWith('z'))
        {
            return true;
        }

        int tIndex = input.IndexOf('T');
        if (tIndex < 0)
        {
            return false;
        }

        return input.LastIndexOf('+') > tIndex || input.LastIndexOf('-') > tIndex;
    }
}
