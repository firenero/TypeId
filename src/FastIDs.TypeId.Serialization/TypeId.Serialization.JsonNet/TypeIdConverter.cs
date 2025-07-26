using Newtonsoft.Json;

namespace FastIDs.TypeId.Serialization.JsonNet;

public class TypeIdConverter : JsonConverter<TypeId>
{
    public override void WriteJson(JsonWriter writer, TypeId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override TypeId ReadJson(JsonReader reader, Type objectType, TypeId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            return reader.Value is string val ? TypeId.Parse(val) : default;
        }
        catch (FormatException ex)
        {
            throw JsonSerializationExceptionFactory.Create(reader, ex.Message, ex);
        }
    }
}