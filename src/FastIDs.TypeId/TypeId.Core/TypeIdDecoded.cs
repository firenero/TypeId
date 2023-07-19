using System.Runtime.InteropServices;
using UUIDNext;
using UUIDNext.Generator;

namespace FastIDs.TypeId;

[StructLayout(LayoutKind.Auto)]
public readonly struct TypeIdDecoded : IEquatable<TypeIdDecoded>
{
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
    
    /// <summary>
    /// Returns the ID generation timestamp.
    /// </summary>
    /// <returns>DateTimeOffset representing the ID generation timestamp.</returns>
    public DateTimeOffset GetTimestamp()
    {
        var (timestampMs, _) = UuidV7Generator.Decode(Id);
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

    public override string ToString()
    {
        Span<char> suffixChars = stackalloc char[Base32Constants.EncodedLength];
        GetSuffix(suffixChars);

        return Type.Length != 0
            ? $"{Type}_{suffixChars}"
            : suffixChars.ToString();
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
    public static TypeIdDecoded New(string type) => FromUuidV7(type, Uuid.NewSequential());
    
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
    public static TypeIdDecoded New(string type, bool validateType) => validateType ? New(type) : new TypeIdDecoded(type, Uuid.NewSequential());

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
        if (!TypeIdParser.ValidateTypeAlphabet(type))
            throw new FormatException("Type must contain only lowercase letters.");

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