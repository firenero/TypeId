using System.Text.Json;

namespace FastIDs.TypeId.Serialization.SystemTextJson;

public static class Extensions
{
    public static JsonSerializerOptions ConfigureForTypeId(this JsonSerializerOptions options)
    {
        options.Converters.Add(new TypeIdConverterFactory());
        options.Converters.Add(new TypeIdDecodedConverterFactory());
        return options;
    }
}