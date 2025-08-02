using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

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
[TypeConverter(typeof(TypeIdTypeConverter))]
[StructLayout(LayoutKind.Auto)]
public readonly struct TypeId : IEquatable<TypeId>, ISpanFormattable, IUtf8SpanFormattable, IComparable<TypeId>, IComparable
{
    private readonly string _str;

    internal TypeId(string str)
    {
        _str = str;
    }

    private int SeparatorIndex => _str.Length - TypeIdConstants.IdLength - 1;

    /// <summary>
    /// Returns a value indicating whether the TypeId has the specified type.
    /// </summary>
    /// <param name="type">The type to compare.</param>
    /// <returns><c>true</c> if the TypeId has the specified type; otherwise, <c>false</c>.</returns>
    public bool HasType(string type) => HasType(type.AsSpan());

    /// <summary>
    /// Returns a value indicating whether the TypeId has the specified type.
    /// </summary>
    /// <param name="type">The type to compare.</param>
    /// <returns><c>true</c> if the TypeId has the specified type; otherwise, <c>false</c>.</returns>
    public bool HasType(ReadOnlySpan<char> type)
    {
        var thisTypeLen = SeparatorIndex > 0 ? SeparatorIndex : 0;

        return type.SequenceEqual(_str.AsSpan(0, thisTypeLen));
    }

    /// <summary>
    /// Returns a string representation of the TypeId.
    /// </summary>
    /// <returns>A string representation of the TypeId.</returns>
    public override string ToString() => _str;

    /// <summary>
    /// Returns a string representation of the TypeId.
    /// </summary>
    /// <param name="format">Format string. Can be empty.</param>
    /// <param name="formatProvider">Format provider. Can be null.</param>
    /// <returns>Formatted string representation of the TypeId.</returns>
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
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => 
        destination.TryWrite($"{_str}", out charsWritten);

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
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => 
        Utf8.TryWrite(utf8Destination, $"{_str}", out bytesWritten);

    /// <summary>
    /// A type component of the TypeId.
    /// </summary>
    public ReadOnlySpan<char> Type => SeparatorIndex > 0 ? _str.AsSpan(0, SeparatorIndex) : ReadOnlySpan<char>.Empty;

    /// <summary>
    /// An encoded UUIDv7 component of the TypeId.
    /// </summary>
    public ReadOnlySpan<char> Suffix => _str.AsSpan(SeparatorIndex + 1);

    /// <summary>
    /// Decodes the TypeId into components <see cref="TypeIdDecoded"/> struct.
    /// </summary>
    /// <returns>Decoded TypeId.</returns>
    public TypeIdDecoded Decode()
    {
        var idSpan = _str.AsSpan(SeparatorIndex + 1);
        Span<byte> decoded = stackalloc byte[Base32Constants.DecodedLength];

        var canDecode = Base32.TryDecode(idSpan, decoded);
        Debug.Assert(canDecode, "Id is not a valid Base32 string. Should never happen because it was validated before.");

        TypeIdParser.FormatUuidBytes(decoded);

        var type = SeparatorIndex > 0 ? _str[..SeparatorIndex] : "";
        return new TypeIdDecoded(type, new Guid(decoded));
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
    public static TypeIdDecoded New(string type) => TypeIdDecoded.New(type);

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
    public static TypeIdDecoded New(string type, bool validateType) => TypeIdDecoded.New(type, validateType);

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
    public static TypeIdDecoded FromUuidV7(string type, Guid uuidV7) => TypeIdDecoded.FromUuidV7(type, uuidV7);

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
    public static TypeIdDecoded FromUuidV7(string type, Guid uuidV7, bool validateType) => TypeIdDecoded.FromUuidV7(type, uuidV7, validateType);

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
        var separatorIdx = input.LastIndexOf('_');
        if (separatorIdx == 0)
            throw new FormatException("Type separator must be omitted if there is no type present.");
        if (separatorIdx > TypeIdConstants.MaxTypeLength)
            throw new FormatException($"Type can be at most {TypeIdConstants.MaxTypeLength} characters long.");

        var typeSpan = separatorIdx != -1 ? input.AsSpan(0, separatorIdx) : ReadOnlySpan<char>.Empty;
        var typeError = TypeIdParser.ValidateType(typeSpan);
        if (typeError is not TypeIdParser.TypeError.None)
            throw new FormatException(typeError.ToErrorMessage());

        var idSpan = input.AsSpan(separatorIdx + 1);
        if (idSpan.Length != TypeIdConstants.IdLength)
            throw new FormatException($"Id must be {TypeIdConstants.IdLength} characters long.");
        if (idSpan[0] > '7')
            throw new FormatException("The maximum possible suffix for TypeId is '7zzzzzzzzzzzzzzzzzzzzzzzzz'");

        if (!Base32.IsValid(idSpan))
            throw new FormatException("Id is not a valid Base32 string.");

        return new(input);
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
        var separatorIdx = input.LastIndexOf('_');
        if (separatorIdx is 0 or > TypeIdConstants.MaxTypeLength)
            return Error(out result);
        
        var typeSpan = separatorIdx != -1 ? input.AsSpan(0, separatorIdx) : ReadOnlySpan<char>.Empty;
        if (TypeIdParser.ValidateType(typeSpan) is not TypeIdParser.TypeError.None)
            return Error(out result);

        var idSpan = input.AsSpan(separatorIdx + 1);
        if (idSpan.Length != TypeIdConstants.IdLength || idSpan[0] > '7')
            return Error(out result);

        if (!Base32.IsValid(idSpan))
            return Error(out result);

        result = new(input);
        return true;
    }

    public bool Equals(TypeId other) => _str == other._str;

    public override bool Equals(object? obj) => obj is TypeId other && Equals(other);

    public override int GetHashCode() => _str.GetHashCode();

    public static bool operator ==(TypeId left, TypeId right) => left.Equals(right);

    public static bool operator !=(TypeId left, TypeId right) => !left.Equals(right);
    
    public static bool operator <(TypeId left, TypeId right) => left.CompareTo(right) < 0;
    
    public static bool operator <=(TypeId left, TypeId right) => left.CompareTo(right) <= 0;
    
    public static bool operator >(TypeId left, TypeId right) => left.CompareTo(right) > 0;
    
    public static bool operator >=(TypeId left, TypeId right) => left.CompareTo(right) >= 0;
    
    public int CompareTo(TypeId other) => string.Compare(_str, other._str, StringComparison.Ordinal);

    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj)) 
            return 1;
        
        return obj is TypeId other 
            ? CompareTo(other) 
            : throw new ArgumentException($"Object must be of type {nameof(TypeId)}");
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Error(out TypeId result)
    {
        result = default;
        return false;
    }
}