using Newtonsoft.Json;

namespace FastIDs.TypeId.Serialization.JsonNet;

public static class Extensions
{
    public static JsonSerializerSettings ConfigureForTypeId(this JsonSerializerSettings settings)
    {
        settings.Converters.Add(new TypeIdConverter());
        settings.Converters.Add(new TypeIdDecodedConverter());
        return settings;
    }
}