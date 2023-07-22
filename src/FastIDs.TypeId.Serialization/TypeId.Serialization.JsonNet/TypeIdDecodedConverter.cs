using Newtonsoft.Json;

namespace FastIDs.TypeId.Serialization.JsonNet;

public class TypeIdDecodedConverter : JsonConverter<TypeIdDecoded>
{
    public override void WriteJson(JsonWriter writer, TypeIdDecoded value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override TypeIdDecoded ReadJson(JsonReader reader, Type objectType, TypeIdDecoded existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.Value is string val ? TypeId.Parse(val).Decode() : default;
    }
}