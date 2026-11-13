using System.Linq.Expressions;

namespace FastIDs.TypeId.EntityFramework;

/// <summary>
/// Shared helpers used by the converters and value generators.
/// </summary>
/// <remarks>
/// All conversions are implemented as static methods and referenced from the converter expression trees by
/// <see cref="Expression.Call(System.Reflection.MethodInfo, Expression, Expression)"/>.
/// EF Core inlines these expressions into the compiled query shapers, so reading an entity costs a single static call
/// per value, and no closure or delegate indirection is involved.
/// </remarks>
internal static class TypeIdConversions
{
    /// <summary>
    /// Maximum length of the string representation of a TypeId: 63 chars of type + separator + 26 chars of suffix.
    /// </summary>
    public const int MaxStringLength = 63 + 1 + 26;

    /// <summary>
    /// Validates the type once, so conversions of fixed-type values don't need to validate it again.
    /// </summary>
    /// <exception cref="FormatException">The type is not a valid TypeId type.</exception>
    public static string ValidateType(string type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Throws FormatException with a descriptive message for invalid types.
        _ = TypeIdDecoded.FromUuidV7(type, Guid.Empty);
        return type;
    }

    public static Expression<Func<TModel, TProvider>> BindType<TModel, TProvider>(
        Func<TModel, string, TProvider> conversion, string type)
    {
        var parameter = Expression.Parameter(typeof(TModel), "value");
        var body = Expression.Call(conversion.Method, parameter, Expression.Constant(type, typeof(string)));
        return Expression.Lambda<Func<TModel, TProvider>>(body, parameter);
    }

    public static int GetHashCode(TypeId id) => id == default ? 0 : id.GetHashCode();

    public static Guid TypeIdToGuid(TypeId id, string type)
    {
        // default(TypeId) has no underlying string, so it can't be inspected.
        if (id == default || !id.HasType(type))
            ThrowTypeMismatch(id == default ? null : id.ToString(), type);

        return id.Decode().Id;
    }

    public static TypeId GuidToTypeId(Guid uuid, string type) =>
        TypeIdDecoded.FromUuidV7(type, uuid, validateType: false).Encode();

    public static Guid TypeIdDecodedToGuid(TypeIdDecoded id, string type)
    {
        if (!id.HasType(type))
            ThrowTypeMismatch(id.Type is null ? null : id.ToString(), type);

        return id.Id;
    }

    public static TypeIdDecoded GuidToTypeIdDecoded(Guid uuid, string type) =>
        TypeIdDecoded.FromUuidV7(type, uuid, validateType: false);

    private static void ThrowTypeMismatch(string? actual, string expected) =>
        throw new InvalidOperationException(
            actual is null
                ? $"Cannot store a default TypeId in a column configured for type '{expected}'."
                : $"Cannot store TypeId '{actual}' in a column configured for type '{expected}'. " +
                  "A GUID column doesn't store the type, so the TypeId must have exactly the configured type.");
}
