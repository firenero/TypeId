using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UUIDNext;

namespace FastIDs.TypeId;

/// <summary>
/// Type-safe extension of UUIDv7.
/// </summary>
/// <remarks>
/// Example TypeId format:
/// <code>
///   user_2x4y6z8a0b1c2d3e4f5g6h7j8k
///   └──┘ └────────────────────────┘
///   type    uuid suffix (base32)
/// </code>
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public readonly struct TypeId : IEquatable<TypeId>
{
    /// <summary>
    /// Type of the ID. Can be empty.
    /// </summary>
    public string Type { get; }
    
    /// <summary>
    /// UUIDv7 ID part of the TypeId.
    /// </summary>
    public Guid Id { get; }

    private TypeId(string type, Guid id)
    {
        Type = type;
        Id = id;
    }

    /// <summary>
    /// Generates new TypeId with the specified type and random UUIDv7.
    /// </summary>
    /// <param name="type">Type of the ID. Can be empty.</param>
    /// <returns>New TypeId with the specified type and random UUIDv7.</returns>
    /// <exception cref="FormatException">Thrown when type is not valid. Type must contain only lowercase ASCII letters and can be at most 63 characters long.</exception>
    /// <remarks>
    /// This method validates the type. If you are sure that type is valid use <see cref="New(string, bool)"/> to skip type validation.
    /// </remarks>
    public static TypeId New(string type) => FromUuidV7(type, Uuid.NewSequential());
    
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
    public static TypeId New(string type, bool validateType) => validateType ? New(type) : new TypeId(type, Uuid.NewSequential());

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
    public static TypeId FromUuidV7(string type, Guid uuidV7)
    {
        if (type.Length > TypeIdConstants.MaxTypeLength)
            throw new FormatException($"Type can be at most {TypeIdConstants.MaxTypeLength} characters long.");
        if (!ValidateTypeAlphabet(type))
            throw new FormatException("Type must contain only lowercase letters.");

        return new TypeId(type, uuidV7);
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
    public static TypeId FromUuidV7(string type, Guid uuidV7, bool validateType) => validateType
        ? FromUuidV7(type, uuidV7)
        : new TypeId(type, uuidV7);

    /// <summary>
    /// Returns ID part of the TypeId as an encoded string.
    /// </summary>
    /// <returns>ID part of the TypeId as an encoded string.</returns>
    public string GetSuffix()
    {
        Span<byte> idBytes = stackalloc byte[16];
        Id.TryWriteBytes(idBytes);

        FormatUuidBytes(idBytes);
        
        return Base32.Encode(idBytes);
    }

    /// <summary>
    /// Returns encoded string representation of the TypeId.
    /// </summary>
    /// <returns>Encoded string representation of the TypeId.</returns>
    public override string ToString() => Type.Length > 0 ? $"{Type}_{GetSuffix()}" : GetSuffix();

    /// <summary>
    /// Parses the specified string into a TypeId.
    /// </summary>
    /// <param name="input">String representation of the TypeId.</param>
    /// <returns>TypeId instance.</returns>
    /// <exception cref="FormatException">Thrown when the specified string is not a valid TypeId.</exception>
    /// <remarks>
    /// Example TypeId format:
    /// <code>
    ///   user_2x4y6z8a0b1c2d3e4f5g6h7j8k
    ///   └──┘ └────────────────────────┘
    ///   type    uuid suffix (base32)
    /// </code>
    /// </remarks>
    public static TypeId Parse(string input)
    {
        var separatorIdx = input.IndexOf('_');
        if (separatorIdx == 0)
            throw new FormatException("Type separator must be omitted if there is no type present.");
        if (separatorIdx > TypeIdConstants.MaxTypeLength)
            throw new FormatException($"Type can be at most {TypeIdConstants.MaxTypeLength} characters long.");

        var typeSpan = separatorIdx != -1 ? input.AsSpan(0, separatorIdx) : ReadOnlySpan<char>.Empty;
        if (!ValidateTypeAlphabet(typeSpan))
            throw new FormatException("Type must contain only lowercase letters.");

        var idSpan = input.AsSpan(separatorIdx + 1);
        if (idSpan.Length != TypeIdConstants.IdLength)
            throw new FormatException($"Id must be {TypeIdConstants.IdLength} characters long.");
        if (idSpan[0] > '7')
            throw new FormatException("The maximum possible suffix for TypeId is '7zzzzzzzzzzzzzzzzzzzzzzzzz'");
        
        Span<byte> decoded = stackalloc byte[Base32Constants.DecodedLength];
        if (!Base32.TryDecode(idSpan, decoded))
            throw new FormatException("Id is not a valid Base32 string.");
 
        FormatUuidBytes(decoded);

        return new TypeId(typeSpan.ToString(), new Guid(decoded));
    }

    /// <summary>
    /// Tries to parse the specified string into a TypeId.
    /// </summary>
    /// <param name="input">String representation of the TypeId.</param>
    /// <param name="result">Contains parsed TypeId if the method returns true.</param>
    /// <returns>True if the specified string was successfully parsed into a TypeId; otherwise, false.</returns>
    /// <remarks>
    /// Example TypeId format:
    /// <code>
    ///   user_2x4y6z8a0b1c2d3e4f5g6h7j8k
    ///   └──┘ └────────────────────────┘
    ///   type    uuid suffix (base32)
    /// </code>
    /// </remarks>
    public static bool TryParse(string input, out TypeId result)
    {
        var separatorIdx = input.IndexOf('_');
        if (separatorIdx is 0 or > TypeIdConstants.MaxTypeLength)
            return Error(out result);

        var typeSpan = separatorIdx != -1 ? input.AsSpan(0, separatorIdx) : ReadOnlySpan<char>.Empty;
        if (!ValidateTypeAlphabet(typeSpan))
            return Error(out result);

        var idSpan = input.AsSpan(separatorIdx + 1);
        if (idSpan.Length != TypeIdConstants.IdLength || idSpan[0] > '7')
            return Error(out result);

        Span<byte> decoded = stackalloc byte[Base32Constants.DecodedLength];
        if (!Base32.TryDecode(idSpan, decoded))
            return Error(out result);
        
        FormatUuidBytes(decoded);

        result = new TypeId(typeSpan.ToString(), new Guid(decoded));
        return true;
    }

    /// <summary>
    /// Returns a value indicating whether this instance and a specified TypeId object represent the same value.
    /// </summary>
    /// <param name="other">The TypeId to compare to this instance.</param>
    /// <returns>True if <paramref name="other"/> is equal to this instance; otherwise, false.</returns>
    public bool Equals(TypeId other) => Type == other.Type && Id.Equals(other.Id);

    /// <summary>
    /// Returns a value indicating whether this instance and a specified object represent the same value.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns>True if <paramref name="obj"/> is a TypeId and equal to this instance; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is TypeId other && Equals(other);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(Type, Id);

    public static bool operator ==(TypeId left, TypeId right) => left.Equals(right);

    public static bool operator !=(TypeId left, TypeId right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Error(out TypeId result)
    {
        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ValidateTypeAlphabet(ReadOnlySpan<char> type)
    {
        foreach (var c in type)
        {
            if (c is < 'a' or > 'z')
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FormatUuidBytes(Span<byte> bytes)
    {
        bytes[..4].Reverse();
        (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
        (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
    }
}