using System.Runtime.InteropServices;
using System.Text.Unicode;
using FastIDs.TypeId.Uuid;

namespace FastIDs.TypeId;

[StructLayout(LayoutKind.Auto)]
public readonly struct TypeIdDecoded : IEquatable<TypeIdDecoded>, ISpanFormattable, IUtf8SpanFormattable
{
    private static readonly UuidGenerator UuidGenerator = new();
    
    /// <summary>
    /// The type part of the TypeId.
    /// </summary>
    public string Type { get; }
    
    /// <summary>
    /// The ID part of the TypeId.
    /// </summary>
    public Guid Id { get; }
    
    internal TypeIdDecoded(string type, Guid id)
    {
        Type = type;
        Id = id;
    }
    
    /// <summary>
    /// Returns the ID part of the TypeId as an encoded string.
    /// </summary>
    /// <returns>ID part of the TypeId as an encoded string.</returns>
    public string GetSuffix()
    {
        Span<char> suffixChars = stackalloc char[Base32Constants.EncodedLength];
        GetSuffix(suffixChars);
        return suffixChars.ToString();
    }

    /// <summary>
    /// Returns the ID part of the TypeId as an encoded string.
    /// </summary>
    /// <param name="output">When this method returns, <paramref name="output"/> contains the encoded ID part of the TypeId.</param>
    /// <returns>Number of characters written to <paramref name="output"/>.</returns>   
    public int GetSuffix(Span<char> output)
    {
        Span<byte> idBytes = stackalloc byte[Base32Constants.DecodedLength];
        Id.TryWriteBytes(idBytes);

        TypeIdParser.FormatUuidBytes(idBytes);

        return Base32.Encode(idBytes, output);
    }
    
    public int GetSuffix(Span<byte> utf8Output)
    {
        Span<byte> idBytes = stackalloc byte[Base32Constants.DecodedLength];
        Id.TryWriteBytes(idBytes);

        TypeIdParser.FormatUuidBytes(idBytes);

        return Base32.Encode(idBytes, utf8Output);
    }

    /// <summary>
    /// Returns the ID generation timestamp.
    /// </summary>
    /// <returns>DateTimeOffset representing the ID generation timestamp.</returns>
    public DateTimeOffset GetTimestamp()
    {
        var (timestampMs, _) = UuidDecoder.Decode(Id);
        return DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
    }

    /// <summary>
    /// Encodes the TypeId components into a TypeId struct.
    /// </summary>
    /// <returns>A TypeId struct that contains the encoded TypeId.</returns>
    public TypeId Encode() => new(ToString());

    /// <summary>
    /// Returns a value indicating whether the TypeId has the specified type.
    /// </summary>
    /// <param name="type">The type to compare.</param>
    /// <returns><c>true</c> if the TypeId has the specified type; otherwise, <c>false</c>.</returns>
    public bool HasType(string type) => type == Type;

    /// <summary>
    /// Returns a value indicating whether the TypeId has the specified type.
    /// </summary>
    /// <param name="type">The type to compare.</param>
    /// <returns><c>true</c> if the TypeId has the specified type; otherwise, <c>false</c>.</returns>
    public bool HasType(ReadOnlySpan<char> type) => type.Equals(Type.AsSpan(), StringComparison.Ordinal);

    /// <summary>
    /// Returns a string that represents the TypeId value.
    /// </summary>
    /// <returns>Formatted string.</returns>
    public override string ToString()
    {
        Span<char> suffixChars = stackalloc char[Base32Constants.EncodedLength];
        GetSuffix(suffixChars);

        return Type.Length != 0
            ? $"{Type}_{suffixChars}"
            : suffixChars.ToString();
    }

    /// <summary>
    /// Returns a string that represents the TypeId value.
    /// </summary>
    /// <param name="format">Format string. Can be empty.</param>
    /// <param name="formatProvider">Format provider. Can be null.</param>
    /// <returns>Formatted string.</returns>
    /// <remarks>
    /// This method ignores <paramref name="format"/> and <paramref name="formatProvider"/> parameters and outputs the same result as <see cref="ToString()"/>.
    /// </remarks>
    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();
    
    /// <summary>
    /// Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
    /// <param name="format">A span containing the characters that represent a standard or custom format string. Can be empty.</param>
    /// <param name="provider">Format provider. Can be null.</param>
    /// <returns><c>true</c> if the formatting was successful; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method ignores <paramref name="format"/> and <paramref name="provider"/> parameters.
    /// </remarks>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;
        if (Type.Length != 0)
        {
            if (!destination.TryWrite($"{Type}_", out charsWritten))
                return false;
        }

        var suffixSpan = destination[charsWritten..];
        if (suffixSpan.Length < Base32Constants.EncodedLength)
            return false;
        
        var suffixCharsWritten = GetSuffix(suffixSpan);
        charsWritten += suffixCharsWritten;

        return true;
    }

    /// <summary>
    /// Tries to format the value of the current instance into the provided span of bytes in UTF-8 encoding.
    /// </summary>
    /// <param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes in UTF-8 encoding.</param>
    /// <param name="bytesWritten">When this method returns, contains the number of bytes that were written in <paramref name="utf8Destination"/>.</param>
    /// <param name="format">A span containing the characters that represent a standard or custom format string. Can be empty.</param>
    /// <param name="provider">Format provider. Can be null.</param>
    /// <returns><c>true</c> if the formatting was successful; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method ignores <paramref name="format"/> and <paramref name="provider"/> parameters.
    /// </remarks>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        bytesWritten = 0;
        if (Type.Length != 0)
        {
            if (!Utf8.TryWrite(utf8Destination, $"{Type}_", out bytesWritten))
                return false;
        }

        var suffixSpan = utf8Destination[bytesWritten..];
        if (suffixSpan.Length < Base32Constants.EncodedLength)
            return false;
        
        var suffixBytesWritten = GetSuffix(suffixSpan);
        bytesWritten += suffixBytesWritten;

        return true;
    }

    public bool Equals(TypeIdDecoded other) => Type == other.Type && Id.Equals(other.Id);

    public override bool Equals(object? obj) => obj is TypeIdDecoded other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Type, Id);

    public static bool operator ==(TypeIdDecoded left, TypeIdDecoded right) => left.Equals(right);

    public static bool operator !=(TypeIdDecoded left, TypeIdDecoded right) => !left.Equals(right);

    /// <summary>
    /// Generates new TypeId with the specified type and random UUIDv7.
    /// </summary>
    /// <param name="type">Type of the ID. Can be empty.</param>
    /// <returns>New TypeId with the specified type and random UUIDv7.</returns>
    /// <exception cref="FormatException">Thrown when type is not valid. Type must contain only lowercase ASCII letters and can be at most 63 characters long.</exception>
    /// <remarks>
    /// This method validates the type. If you are sure that type is valid use <see cref="New(string, bool)"/> to skip type validation.
    /// </remarks>
    public static TypeIdDecoded New(string type) => FromUuidV7(type, UuidGenerator.New());
    
    /// <summary>
    /// Generates new TypeId with the specified type and random UUIDv7. If <paramref name="validateType"/> is false, type is not validated.
    /// </summary>
    /// <param name="type">Type of the ID. Can be empty.</param>
    /// <param name="validateType">If true, type is validated. If false, type is not validated.</param>
    /// <returns>New TypeId with the specified type and random UUIDv7.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="validateType"/> is set to true and <paramref name="type"/> is not valid. Type must contain only lowercase ASCII letters and can be at most 63 characters long.</exception>
    /// <remarks>
    /// Use this method with <paramref name="validateType"/> set to false when you are sure that <paramref name="type"/> is valid.
    /// This method is a bit faster than <see cref="New(string)"/> (especially for longer types) because it skips type validation.
    /// </remarks>
    public static TypeIdDecoded New(string type, bool validateType) => validateType ? New(type) : new TypeIdDecoded(type, UuidGenerator.New());

    /// <summary>
    /// Generates new TypeId with the specified type and UUIDv7.
    /// </summary>
    /// <param name="type">Type of the ID. Can be empty.</param>
    /// <param name="uuidV7">UUIDv7 ID part of the TypeId.</param>
    /// <returns>New TypeId with the specified type and UUIDv7.</returns>
    /// <exception cref="FormatException">Thrown when type is not valid. Type must contain only lowercase ASCII letters and can be at most 63 characters long.</exception>
    /// <remarks>
    /// <paramref name="uuidV7"/> must be a valid UUIDv7. <see cref="Guid.NewGuid"/> method generates UUIDv4 which is not valid UUIDv7.
    /// <br/><br/>
    /// This method validates the type. If you are sure that type is valid use <see cref="New(string, bool)"/> to skip type validation.
    /// </remarks>
    public static TypeIdDecoded FromUuidV7(string type, Guid uuidV7)
    {
        if (type.Length > TypeIdConstants.MaxTypeLength)
            throw new FormatException($"Type can be at most {TypeIdConstants.MaxTypeLength} characters long.");
        var typeError = TypeIdParser.ValidateType(type);
        if (typeError is not TypeIdParser.TypeError.None)
            throw new FormatException(typeError.ToErrorMessage());

        return new TypeIdDecoded(type, uuidV7);
    }

    /// <summary>
    /// Generates new TypeId with the specified type and UUIDv7. If <paramref name="validateType"/> is false, type is not validated.
    /// </summary>
    /// <param name="type">Type of the ID. Can be empty.</param>
    /// <param name="uuidV7">UUIDv7 ID part of the TypeId.</param>
    /// <param name="validateType">If true, type is validated. If false, type is not validated.</param>
    /// <returns>New TypeId with the specified type and UUIDv7.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="validateType"/> is set to true and <paramref name="type"/> is not valid. Type must contain only lowercase ASCII letters and can be at most 63 characters long.</exception>
    /// <remarks>
    /// <paramref name="uuidV7"/> must be a valid UUIDv7. <see cref="Guid.NewGuid"/> method generates UUIDv4 which is not valid UUIDv7.
    /// <br/><br/>
    /// Use this method with <paramref name="validateType"/> set to false when you are sure that <paramref name="type"/> is valid.
    /// This method is a bit faster than <see cref="New(string)"/> (especially for longer types) because it skips type validation.
    /// </remarks>
    public static TypeIdDecoded FromUuidV7(string type, Guid uuidV7, bool validateType) => validateType
        ? FromUuidV7(type, uuidV7)
        : new TypeIdDecoded(type, uuidV7);
}