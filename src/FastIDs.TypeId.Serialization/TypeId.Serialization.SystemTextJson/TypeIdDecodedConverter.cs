using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastIDs.TypeId.Serialization.SystemTextJson;

public class TypeIdDecodedConverter : JsonConverter<TypeIdDecoded>
{
    public override TypeIdDecoded Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var val = reader.GetString();
        return val is not null ? TypeId.Parse(val).Decode() : default;
    }

    public override void Write(Utf8JsonWriter writer, TypeIdDecoded value, JsonSerializerOptions options)
    {
        var totalLength = value.Type.Length + 1 + 26;
        Span<char> buffer = stackalloc char[totalLength];
        
        value.Type.AsSpan().CopyTo(buffer);
        buffer[value.Type.Length] = '_';
        value.GetSuffix(buffer[(value.Type.Length + 1)..]);

        writer.WriteStringValue(buffer);
    }
}