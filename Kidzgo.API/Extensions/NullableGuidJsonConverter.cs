using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kidzgo.API.Extensions;

public class NullableGuidJsonConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            
            // Convert empty string to null
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            // Try to parse as Guid
            if (Guid.TryParse(stringValue, out var guid))
            {
                return guid;
            }
        }

        // If it's already a Guid token, read it directly
        try
        {
            return reader.GetGuid();
        }
        catch
        {
            return null;
        }
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value);
    }
}

// Factory to automatically apply to all Guid? types
public class NullableGuidJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(Guid?);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return new NullableGuidJsonConverter();
    }
}

