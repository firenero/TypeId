using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastIDs.TypeId.Serialization.SystemTextJson;

public class TypeIdConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(TypeId) || typeToConvert == typeof(TypeId?);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a converter for the specific type
        return (JsonConverter?)Activator.CreateInstance(
            typeof(TypeIdConverter<>).MakeGenericType(typeToConvert),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            null,
            null) ?? throw new ArgumentException($"Could not create converter of type {typeToConvert}");
    }

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Instantiated via reflection")]
    private sealed class TypeIdConverter<T> : JsonConverter<T>
    {
        // Determine if the type is nullable
        private static readonly bool IsNullable = Nullable.GetUnderlyingType(typeof(T)) != null;

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Handle null values
            if (reader.TokenType == JsonTokenType.Null)
            {
                if (IsNullable)
                    return default;
                throw new JsonException($"Cannot convert null to {typeof(T)}.");
            }

            var val = reader.GetString();
            if (!string.IsNullOrEmpty(val))
            {
                var typeId = TypeId.Parse(val);
                return (T)(object)typeId;
            }

            throw new JsonException($"Expected a non-null, non-empty string for {typeof(T)}.");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // Handle null values
            if (value == null)
            {
                if (IsNullable)
                {
                    writer.WriteNullValue();
                    return;
                }

                throw new JsonException($"Cannot write null value for {typeof(T)}.");
            }

            var typedValue = (TypeId)(object)value;
            writer.WriteStringValue(typedValue.ToString());
        }

        public override T ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            // Read property name as string
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var val = reader.GetString();
                if (!string.IsNullOrEmpty(val))
                {
                    var typeId = TypeId.Parse(val);
                    return (T)(object)typeId;
                }

                throw new JsonException($"Expected a non-null, non-empty string for property name of {typeof(T)}.");
            }

            throw new JsonException($"Expected a property name token for {typeof(T)}.");
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value == null) throw new JsonException($"Cannot write null as a property name for {typeof(T)}.");

            var typedValue = (TypeId)(object)value;
            writer.WritePropertyName(typedValue.ToString());
        }
    }
}