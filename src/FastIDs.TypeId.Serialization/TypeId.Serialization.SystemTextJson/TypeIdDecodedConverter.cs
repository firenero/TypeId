using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastIDs.TypeId.Serialization.SystemTextJson;

public class TypeIdDecodedConverter : JsonConverter<TypeIdDecoded>
{
    public override TypeIdDecoded Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadTypeId(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, TypeIdDecoded value, JsonSerializerOptions options)
    {
        var totalLength = value.Type.Length + 1 + 26;
        Span<char> buffer = stackalloc char[totalLength];

        CopyValueToBuffer(value, buffer);

        writer.WriteStringValue(buffer);
    }

    public override TypeIdDecoded ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadTypeId(ref reader);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] TypeIdDecoded value, JsonSerializerOptions options)
    {
        var totalLength = value.Type.Length + 1 + 26;
        Span<char> buffer = stackalloc char[totalLength];

        CopyValueToBuffer(value, buffer);

        writer.WritePropertyName(buffer);
    }

    private static TypeIdDecoded ReadTypeId(ref Utf8JsonReader reader)
    {
        var val = reader.GetString();
        return val is not null ? TypeId.Parse(val).Decode() : default;
    }

    private static void CopyValueToBuffer(TypeIdDecoded value, Span<char> buffer)
    {
        value.Type.AsSpan().CopyTo(buffer);
        buffer[value.Type.Length] = '_';
        value.GetSuffix(buffer[(value.Type.Length + 1)..]);
    }
}