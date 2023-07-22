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
        writer.WriteStringValue(value.ToString());
    }
}