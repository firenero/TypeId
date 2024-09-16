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

            var val = reader.GetString();
            if (!string.IsNullOrEmpty(val))
            {
                var decoded = TypeId.Parse(val).Decode();
                return (T)(object)decoded;
            }

            throw new JsonException($"Expected a non-null, non-empty string for {typeof(T)}.");
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