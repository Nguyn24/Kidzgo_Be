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
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (DateTime.TryParse(dateString, out var date))
            {
                // If the date is in UTC, convert to Vietnam time
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

            if (DateTime.TryParse(dateString, out var date))
            {
                // If the date is in UTC, convert to Vietnam time
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
