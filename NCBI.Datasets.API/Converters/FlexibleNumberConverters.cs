using System.Text.Json;
using System.Text.Json.Serialization;

namespace NCBI.Datasets.API.Converters;

/// <summary>
/// JSON converter that can handle both string and numeric representations for long values
/// </summary>
public class FlexibleLongConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetInt64();
            
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return 0;
                
                if (long.TryParse(stringValue, out var longValue))
                    return longValue;
                
                throw new JsonException($"Unable to convert '{stringValue}' to long");
            
            case JsonTokenType.Null:
                return 0;
            
            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when parsing long");
        }
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// JSON converter that can handle both string and numeric representations for int values
/// </summary>
public class FlexibleIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetInt32();
            
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return 0;
                
                if (int.TryParse(stringValue, out var intValue))
                    return intValue;
                
                throw new JsonException($"Unable to convert '{stringValue}' to int");
            
            case JsonTokenType.Null:
                return 0;
            
            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when parsing int");
        }
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// JSON converter that can handle both string and numeric representations for float values
/// </summary>
public class FlexibleFloatConverter : JsonConverter<float>
{
    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetSingle();
            
            case JsonTokenType.String:
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return 0f;
                
                if (float.TryParse(stringValue, out var floatValue))
                    return floatValue;
                
                throw new JsonException($"Unable to convert '{stringValue}' to float");
            
            case JsonTokenType.Null:
                return 0f;
            
            default:
                throw new JsonException($"Unexpected token type {reader.TokenType} when parsing float");
        }
    }

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
