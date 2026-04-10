namespace Kidzgo.Application.Time;

public static class VietnamTime
{
    public const string TimeZoneId = Kidzgo.Domain.Time.VietnamTime.TimeZoneId;

    public static DateTime UtcNow() => Kidzgo.Domain.Time.VietnamTime.UtcNow();
    public static DateTime NowInVietnam() => Kidzgo.Domain.Time.VietnamTime.NowInVietnam();
    public static DateTime TodayStartUtc() => Kidzgo.Domain.Time.VietnamTime.TodayStartUtc();
    public static DateOnly TodayDateOnly() => Kidzgo.Domain.Time.VietnamTime.TodayDateOnly();
    public static DateOnly ToVietnamDateOnly(DateTime value) => Kidzgo.Domain.Time.VietnamTime.ToVietnamDateOnly(value);
    public static TimeOnly ToVietnamTimeOnly(DateTime value) => Kidzgo.Domain.Time.VietnamTime.ToVietnamTimeOnly(value);
    public static DateTime ToVietnamDateTime(DateTime value) => Kidzgo.Domain.Time.VietnamTime.ToVietnamDateTime(value);
    public static DateTime NormalizeToUtc(DateTime value) => Kidzgo.Domain.Time.VietnamTime.NormalizeToUtc(value);
    public static DateTime? NormalizeToUtc(DateTime? value) => Kidzgo.Domain.Time.VietnamTime.NormalizeToUtc(value);
    public static DateTime EnsureUtcKind(DateTime value) => Kidzgo.Domain.Time.VietnamTime.EnsureUtcKind(value);
    public static string FormatUtcAsVietnamOffset(DateTime value) => Kidzgo.Domain.Time.VietnamTime.FormatUtcAsVietnamOffset(value);
    public static string FormatInVietnam(DateTime value, string format) => Kidzgo.Domain.Time.VietnamTime.FormatInVietnam(value, format);
    public static bool TryParseApiDateTime(string? input, out DateTime utcValue) => Kidzgo.Domain.Time.VietnamTime.TryParseApiDateTime(input, out utcValue);
    public static DateTime TreatAsVietnamLocal(DateTime value) => Kidzgo.Domain.Time.VietnamTime.TreatAsVietnamLocal(value);
    public static DateTime EndOfVietnamDayUtc(DateTime value) => Kidzgo.Domain.Time.VietnamTime.EndOfVietnamDayUtc(value);
}
