using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kidzgo.API.Extensions;

public sealed class StringOrStringArrayJsonConverter : JsonConverter<List<string>?>
{
    public override List<string>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            return new List<string> { reader.GetString() ?? string.Empty };
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Value must be a string or an array of strings.");
        }

        var values = new List<string>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return values;
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                continue;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException("Array values must be strings.");
            }

            values.Add(reader.GetString() ?? string.Empty);
        }

        throw new JsonException("Invalid string array.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        List<string>? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value, options);
    }
}
