using System.Text.Json;
using System.Text.Json.Serialization;
using Kidzgo.Application.Time;

namespace Kidzgo.API.Extensions;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            return VietnamTime.NormalizeToUtc(reader.GetDateTime());
        }

        var input = reader.GetString();
        if (VietnamTime.TryParseApiDateTime(input, out var utcValue))
        {
            return utcValue;
        }

        throw new JsonException($"Invalid date time value '{input}'. Use ISO 8601 with offset, e.g. 2026-03-24T22:22:24+07:00.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(VietnamTime.FormatUtcAsVietnamOffset(value));
}

public class NullableDateTimeJsonConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            return VietnamTime.NormalizeToUtc(reader.GetDateTime());
        }

        var input = reader.GetString();
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        if (VietnamTime.TryParseApiDateTime(input, out var utcValue))
        {
            return utcValue;
        }

        throw new JsonException($"Invalid date time value '{input}'. Use ISO 8601 with offset, e.g. 2026-03-24T22:22:24+07:00.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(VietnamTime.FormatUtcAsVietnamOffset(value.Value));
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
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
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

        return DateOnly.FromDateTime(reader.GetDateTime());
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
    }
}
