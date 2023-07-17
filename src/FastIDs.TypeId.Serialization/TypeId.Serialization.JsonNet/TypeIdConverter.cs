﻿using Newtonsoft.Json;

namespace FastIDs.TypeId.Serialization.JsonNet;

public class TypeIdConverter : JsonConverter<TypeId>
{
    public override void WriteJson(JsonWriter writer, TypeId value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override TypeId ReadJson(JsonReader reader, Type objectType, TypeId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.Value is string val ? TypeId.Parse(val) : default;
    }
}