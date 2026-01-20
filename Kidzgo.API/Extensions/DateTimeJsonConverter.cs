using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kidzgo.API.Extensions;

internal static class VietnamTimeZoneHelper
{
    private static readonly Lazy<TimeZoneInfo> _vietnamTimeZone = new(GetVietnamTimeZone);

    public static TimeZoneInfo VietnamTimeZone => _vietnamTimeZone.Value;

    private static TimeZoneInfo GetVietnamTimeZone()
    {
        try
        {
            // Try Windows timezone ID first
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
        catch
        {
            try
            {
                // Try Linux timezone ID
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch
            {
                // Fallback to UTC+7 offset
                return TimeZoneInfo.CreateCustomTimeZone("Vietnam Standard Time", TimeSpan.FromHours(7), "Vietnam Standard Time", "Vietnam Standard Time");
            }
        }
    }
}

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    internal static readonly string[] SupportedFormats = new[]
    {
        "dd/MM/yyyy hh:mm:ss tt",
        "d/M/yyyy hh:mm:ss tt",
        "dd/M/yyyy hh:mm:ss tt",
        "d/MM/yyyy hh:mm:ss tt",
        "yyyy-MM-dd'T'HH:mm:ss",
        "yyyy-MM-dd'T'HH:mm:ss.fff",
        "yyyy-MM-dd'T'HH:mm:ssK",
        "yyyy-MM-dd'T'HH:mm:ss.fffK",
        "O"
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (!string.IsNullOrWhiteSpace(dateString) &&
                DateTime.TryParseExact(
                    dateString,
                    SupportedFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                    out var date))
            {
                // If the date is in UTC, convert to Vietnam time
                if (date.Kind == DateTimeKind.Utc)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(date, VietnamTimeZoneHelper.VietnamTimeZone);
                }
                return date;
            }

            if (!string.IsNullOrWhiteSpace(dateString) &&
                DateTime.TryParse(
                    dateString,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                    out date))
            {
                if (date.Kind == DateTimeKind.Utc)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(date, VietnamTimeZoneHelper.VietnamTimeZone);
                }
                return date;
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        return reader.GetDateTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Convert to Vietnam time zone if it's UTC
        DateTime vietnamTime;
        if (value.Kind == DateTimeKind.Utc)
        {
            vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(value, VietnamTimeZoneHelper.VietnamTimeZone);
        }
        else if (value.Kind == DateTimeKind.Unspecified)
        {
            // Assume it's already in Vietnam time
            vietnamTime = value;
        }
        else
        {
            vietnamTime = TimeZoneInfo.ConvertTime(value, VietnamTimeZoneHelper.VietnamTimeZone);
        }

        // Format: "23/11/2025 10:46:41 PM"
        var formatted = vietnamTime.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
        writer.WriteStringValue(formatted);
    }
}

public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
            {
                return null;
            }

            if (DateTime.TryParseExact(
                dateString,
                DateTimeJsonConverter.SupportedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.RoundtripKind,
                out var date))
            {
                // If the date is in UTC, convert to Vietnam time
                if (date.Kind == DateTimeKind.Utc)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(date, VietnamTimeZoneHelper.VietnamTimeZone);
                }
                return date;
            }

            if (DateTime.TryParse(
                dateString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out date))
            {
                if (date.Kind == DateTimeKind.Utc)
                {
                    return TimeZoneInfo.ConvertTimeFromUtc(date, VietnamTimeZoneHelper.VietnamTimeZone);
                }
                return date;
            }
        }

        var dateTime = reader.GetDateTime();
        return dateTime.Kind == DateTimeKind.Utc
            ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, VietnamTimeZoneHelper.VietnamTimeZone)
            : dateTime;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        // Convert to Vietnam time zone if it's UTC
        DateTime vietnamTime;
        var dateValue = value.Value;
        if (dateValue.Kind == DateTimeKind.Utc)
        {
            vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(dateValue, VietnamTimeZoneHelper.VietnamTimeZone);
        }
        else if (dateValue.Kind == DateTimeKind.Unspecified)
        {
            // Assume it's already in Vietnam time
            vietnamTime = dateValue;
        }
        else
        {
            vietnamTime = TimeZoneInfo.ConvertTime(dateValue, VietnamTimeZoneHelper.VietnamTimeZone);
        }

        // Format: "23/11/2025 10:46:41 PM"
        var formatted = vietnamTime.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
        writer.WriteStringValue(formatted);
    }
}

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (DateOnly.TryParse(dateString, out var date))
            {
                return date;
            }
        }
        else if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        return DateOnly.FromDateTime(reader.GetDateTime());
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        // Format: "2025-11-23" (ISO 8601 date format)
        var formatted = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        writer.WriteStringValue(formatted);
    }
}

public class NullableDateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
            {
                return null;
            }

            if (DateOnly.TryParse(dateString, out var date))
            {
                return date;
            }
        }

        var dateTime = reader.GetDateTime();
        return DateOnly.FromDateTime(dateTime);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        // Format: "2025-11-23" (ISO 8601 date format)
        var formatted = value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        writer.WriteStringValue(formatted);
    }
}
