using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastIDs.TypeId.Serialization.SystemTextJson;

public class TypeIdConverter : JsonConverter<TypeId>
{
    public override TypeId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var val = reader.GetString();
        return val is not null ? TypeId.Parse(val) : default;
    }

    public override void Write(Utf8JsonWriter writer, TypeId value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] TypeId value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }

    public override TypeId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var val = reader.GetString();
        return val is not null ? TypeId.Parse(val) : default;
    }
}