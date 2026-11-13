using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace FastIDs.TypeId.EntityFramework;

/// <summary>
/// Generates new <see cref="TypeId"/> values of a fixed type on the client side.
/// </summary>
/// <remarks>
/// The generator is stateless and thread-safe, a single instance can be shared by all entity types.
/// </remarks>
public sealed class TypeIdValueGenerator : ValueGenerator<TypeId>
{
    private readonly string _type;

    /// <summary>
    /// Creates a generator for TypeIds of the specified type.
    /// </summary>
    /// <param name="type">Type of the generated TypeIds. Can be empty.</param>
    /// <exception cref="FormatException">The type is not a valid TypeId type.</exception>
    public TypeIdValueGenerator(string type)
    {
        _type = TypeIdConversions.ValidateType(type);
    }

    /// <inheritdoc />
    public override bool GeneratesTemporaryValues => false;

    /// <inheritdoc />
    public override TypeId Next(EntityEntry entry) => TypeId.New(_type, validateType: false).Encode();
}

/// <summary>
/// Generates new <see cref="TypeIdDecoded"/> values of a fixed type on the client side.
/// </summary>
/// <remarks>
/// The generator is stateless and thread-safe, a single instance can be shared by all entity types.
/// </remarks>
public sealed class TypeIdDecodedValueGenerator : ValueGenerator<TypeIdDecoded>
{
    private readonly string _type;

    /// <summary>
    /// Creates a generator for TypeIds of the specified type.
    /// </summary>
    /// <param name="type">Type of the generated TypeIds. Can be empty.</param>
    /// <exception cref="FormatException">The type is not a valid TypeId type.</exception>
    public TypeIdDecodedValueGenerator(string type)
    {
        _type = TypeIdConversions.ValidateType(type);
    }

    /// <inheritdoc />
    public override bool GeneratesTemporaryValues => false;

    /// <inheritdoc />
    public override TypeIdDecoded Next(EntityEntry entry) => TypeId.New(_type, validateType: false);
}
