using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastIDs.TypeId.Serialization.SystemTextJson;

public class TypeIdDecodedConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(TypeIdDecoded) || typeToConvert == typeof(TypeIdDecoded?);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a converter for the specific type
        return (JsonConverter?)Activator.CreateInstance(
            typeof(TypeIdDecodedConverter<>).MakeGenericType(typeToConvert),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            null,
            null) ?? throw new ArgumentException($"Could not create converter of type {typeToConvert}");
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "reflection")]
    private sealed class TypeIdDecodedConverter<T> : JsonConverter<T>
    {
        // Determine if the type is nullable
        private static readonly bool IsNullable = Nullable.GetUnderlyingType(typeof(T)) != null;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null values
            if (reader.TokenType == JsonTokenType.Null)
            {
                if (IsNullable) return default;
                throw new JsonException($"Cannot convert null to {typeof(T)}.");
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var val = reader.GetString();
                if (!string.IsNullOrEmpty(val))
                {
                    var decoded = TypeId.Parse(val).Decode();
                    return (T)(object)decoded;
                }

                throw new JsonException($"Expected a non-null, non-empty string for {typeof(T)}.");
            }
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                // Deserialize the object representation
                var jsonObject = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

                if (jsonObject.TryGetProperty("type", out var typeProperty) &&
                    jsonObject.TryGetProperty("id", out var idProperty))
                {
                    var type = typeProperty.GetString();
                    var id = idProperty.GetString();

                    if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(id))
                    {
                        if (!Guid.TryParse(id, out var parsedId))
                            throw new JsonException($"The 'id' property must be a valid UUID for {typeof(T)}.");
                        
                        // Create the TypeIdDecoded instance from type and id
                        var typeId = TypeId.FromUuidV7(type, parsedId);
                        return (T)(object)typeId;
                    }
                    else
                    {
                        throw new JsonException($"The 'type' and 'id' properties must be non-null strings for {typeof(T)}.");
                    }
                }
                else
                {
                    throw new JsonException($"Expected properties 'type' and 'id' in the JSON object for {typeof(T)}.");
                }
            }
            else
            {
                throw new JsonException($"Unexpected token parsing {typeof(T)}. Expected String or StartObject, got {reader.TokenType}.");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // Handle null values
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var typedValue = (TypeIdDecoded)(object)value;

            var totalLength = typedValue.Type.Length + 1 + 26;
            Span<char> buffer = stackalloc char[totalLength];

            CopyValueToBuffer(typedValue, buffer);

            writer.WriteStringValue(buffer);
        }

        public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            return Read(ref reader, typeToConvert, options) ?? throw new JsonException($"Expected a non-null, non-empty string for property name of {typeof(T)}.");
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null) throw new JsonException($"Cannot write null as a property name for {typeof(T)}.");

            var typedValue = (TypeIdDecoded)(object)value;

            var totalLength = typedValue.Type.Length + 1 + 26;
            Span<char> buffer = stackalloc char[totalLength];

            CopyValueToBuffer(typedValue, buffer);

            writer.WritePropertyName(buffer);
        }

        private static void CopyValueToBuffer(in TypeIdDecoded value, Span<char> buffer)
        {
            value.Type.AsSpan().CopyTo(buffer);
            buffer[value.Type.Length] = '_';
            value.GetSuffix(buffer[(value.Type.Length + 1)..]);
        }
    }
}