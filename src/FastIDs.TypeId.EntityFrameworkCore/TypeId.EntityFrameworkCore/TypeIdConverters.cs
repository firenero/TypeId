using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FastIDs.TypeId.EntityFrameworkCore;

/// <summary>
/// Converts <see cref="TypeId"/> to and from its canonical string representation (<c>user_01h455vb4pex5vsknk084sn02q</c>).
/// </summary>
/// <remarks>
/// The column is limited to 90 ASCII characters (the longest possible TypeId) so it can be used as a key or indexed
/// on all providers. Supports every TypeId type, so one column can hold values of different types.
/// Reading validates the stored value.
/// </remarks>
public sealed class TypeIdToStringConverter : ValueConverter<TypeId, string>
{
    private static readonly Expression<Func<TypeId, string>> ToProvider = id => id.ToString();
    private static readonly Expression<Func<string, TypeId>> FromProvider = value => TypeId.Parse(value);

    /// <summary>
    /// Creates a converter with default mapping hints (max length 90, non-unicode).
    /// </summary>
    public TypeIdToStringConverter() : this(null)
    {
    }

    /// <summary>
    /// Creates a converter with the specified mapping hints.
    /// </summary>
    /// <param name="mappingHints">Mapping hints or <c>null</c> to use the default ones (max length 90, non-unicode).</param>
    public TypeIdToStringConverter(ConverterMappingHints? mappingHints)
        : base(ToProvider, FromProvider, mappingHints ?? StringMappingHints.Default)
    {
    }
}

/// <summary>
/// Converts <see cref="TypeIdDecoded"/> to and from its canonical string representation (<c>user_01h455vb4pex5vsknk084sn02q</c>).
/// </summary>
/// <remarks>
/// The column is limited to 90 ASCII characters (the longest possible TypeId) so it can be used as a key or indexed
/// on all providers. Supports every TypeId type, so one column can hold values of different types.
/// Reading validates the stored value.
/// </remarks>
public sealed class TypeIdDecodedToStringConverter : ValueConverter<TypeIdDecoded, string>
{
    private static readonly Expression<Func<TypeIdDecoded, string>> ToProvider = id => id.ToString();
    private static readonly Expression<Func<string, TypeIdDecoded>> FromProvider = value => TypeId.Parse(value).Decode();

    /// <summary>
    /// Creates a converter with default mapping hints (max length 90, non-unicode).
    /// </summary>
    public TypeIdDecodedToStringConverter() : this(null)
    {
    }

    /// <summary>
    /// Creates a converter with the specified mapping hints.
    /// </summary>
    /// <param name="mappingHints">Mapping hints or <c>null</c> to use the default ones (max length 90, non-unicode).</param>
    public TypeIdDecodedToStringConverter(ConverterMappingHints? mappingHints)
        : base(ToProvider, FromProvider, mappingHints ?? StringMappingHints.Default)
    {
    }
}

/// <summary>
/// Converts <see cref="TypeId"/> of a fixed type to and from the UUID part only, so it can be stored in a native UUID column.
/// </summary>
/// <remarks>
/// A UUID column is the most compact representation (16 bytes) and, for UUIDv7, is time-ordered on providers that sort
/// UUIDs by their big-endian bytes (e.g. PostgreSQL, MySQL). SQL Server sorts <c>uniqueidentifier</c>
/// differently, so prefer a string column there if the order of IDs matters.
/// <br/><br/>
/// The type is not stored, so it is a part of the mapping: every value written must have exactly this type.
/// Writing a TypeId of another type throws <see cref="InvalidOperationException"/> instead of silently changing the type.
/// </remarks>
public sealed class TypeIdToGuidConverter : ValueConverter<TypeId, Guid>
{
    /// <summary>
    /// Creates a converter for TypeIds of the specified type.
    /// </summary>
    /// <param name="type">Type of the TypeIds stored in the column. Can be empty.</param>
    /// <exception cref="FormatException">The type is not a valid TypeId type.</exception>
    public TypeIdToGuidConverter(string type) : this(TypeIdConversions.ValidateType(type), default(ConverterMappingHints?))
    {
    }

    private TypeIdToGuidConverter(string validType, ConverterMappingHints? mappingHints)
        : base(
            TypeIdConversions.BindType<TypeId, Guid>(TypeIdConversions.TypeIdToGuid, validType),
            TypeIdConversions.BindType<Guid, TypeId>(TypeIdConversions.GuidToTypeId, validType),
            mappingHints)
    {
        Type = validType;
    }

    /// <summary>
    /// Type of the TypeIds this converter handles.
    /// </summary>
    public string Type { get; }
}

/// <summary>
/// Converts <see cref="TypeIdDecoded"/> of a fixed type to and from the UUID part only, so it can be stored in a native UUID column.
/// </summary>
/// <remarks>
/// See <see cref="TypeIdToGuidConverter"/> for the trade-offs of storing TypeIds as UUIDs.
/// </remarks>
public sealed class TypeIdDecodedToGuidConverter : ValueConverter<TypeIdDecoded, Guid>
{
    /// <summary>
    /// Creates a converter for TypeIds of the specified type.
    /// </summary>
    /// <param name="type">Type of the TypeIds stored in the column. Can be empty.</param>
    /// <exception cref="FormatException">The type is not a valid TypeId type.</exception>
    public TypeIdDecodedToGuidConverter(string type) : this(TypeIdConversions.ValidateType(type), default(ConverterMappingHints?))
    {
    }

    private TypeIdDecodedToGuidConverter(string validType, ConverterMappingHints? mappingHints)
        : base(
            TypeIdConversions.BindType<TypeIdDecoded, Guid>(TypeIdConversions.TypeIdDecodedToGuid, validType),
            TypeIdConversions.BindType<Guid, TypeIdDecoded>(TypeIdConversions.GuidToTypeIdDecoded, validType),
            mappingHints)
    {
        Type = validType;
    }

    /// <summary>
    /// Type of the TypeIds this converter handles.
    /// </summary>
    public string Type { get; }
}

internal static class StringMappingHints
{
    public static readonly ConverterMappingHints Default = new(size: TypeIdConversions.MaxStringLength, unicode: false);
}
