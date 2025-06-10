using System.Text.Json;
using System.Text.Json.Serialization;
using Apparatus.AOT.Reflection;

namespace Pagination.Json;

public class JsonStringEnumConverterProvider : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsEnum;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert.IsEnum)
        {
            return Activator.CreateInstance(typeof(JsonStringEnumConverter<>).MakeGenericType(typeToConvert)) as JsonConverter;
        }
        return null;
    }
}

public class JsonStringEnumConverter<TEnum> : System.Text.Json.Serialization.JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token but got {reader.TokenType}.");
        }

        var enumString = reader.GetString();
        if (string.IsNullOrEmpty(enumString))
        {
            return default;
        }

        var values = EnumHelper.GetEnumInfo<TEnum>();
        foreach (var valueInfo in values)
        {
            var value = valueInfo.Description ?? valueInfo.Name;
            if (value.Equals(enumString, StringComparison.OrdinalIgnoreCase))
            {
                return valueInfo.Value;
            }
        }

        throw new JsonException($"Unknown enum value '{enumString}' for type '{typeof(TEnum).Name}'.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        var valueInfo = EnumHelper.Get(value);
        if (valueInfo == null)
        {
            throw new JsonException($"Unknown enum value '{value}' for type '{typeof(TEnum).Name}'.");
        }

        var enumString = valueInfo.Description ?? valueInfo.Name;
        writer.WriteStringValue(enumString);
    }
}