using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FastIDs.TypeId.EntityFrameworkCore;

/// <summary>
/// Extension methods for configuring TypeId in Entity Framework Core.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Maps every <see cref="TypeId"/> and <see cref="TypeIdDecoded"/> property (including nullable ones) of the model
    /// to a string column (max length 90, non-unicode).
    /// </summary>
    /// <remarks>
    /// Call it from <see cref="DbContext.ConfigureConventions"/>:
    /// <code>
    /// protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    ///     => configurationBuilder.ConfigureForTypeId();
    /// </code>
    /// Individual properties can still override the convention, e.g. with
    /// <see cref="HasTypeIdGuidConversion(PropertyBuilder{TypeId}, string)"/>.
    /// </remarks>
    public static ModelConfigurationBuilder ConfigureForTypeId(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TypeId>()
            .HaveConversion<TypeIdToStringConverter, TypeIdValueComparer>();
        configurationBuilder.Properties<TypeIdDecoded>()
            .HaveConversion<TypeIdDecodedToStringConverter, TypeIdDecodedValueComparer>();
        return configurationBuilder;
    }

    /// <summary>
    /// Stores the property in a native UUID column. The TypeId type isn't stored, so only values of
    /// <paramref name="type"/> can be written to the property.
    /// </summary>
    /// <param name="builder">The property builder.</param>
    /// <param name="type">Type of the TypeIds stored in the property.</param>
    /// <exception cref="FormatException">The type is not a valid TypeId type.</exception>
    /// <seealso cref="TypeIdToGuidConverter"/>
    public static PropertyBuilder<TypeId> HasTypeIdGuidConversion(this PropertyBuilder<TypeId> builder, string type) =>
        builder.HasConversion(new TypeIdToGuidConverter(type), new TypeIdValueComparer());

    /// <inheritdoc cref="HasTypeIdGuidConversion(PropertyBuilder{TypeId}, string)"/>
    public static PropertyBuilder<TypeIdDecoded> HasTypeIdGuidConversion(this PropertyBuilder<TypeIdDecoded> builder, string type) =>
        builder.HasConversion(new TypeIdDecodedToGuidConverter(type), new TypeIdDecodedValueComparer());

    /// <summary>
    /// Generates a new TypeId (UUIDv7) of the specified type on the client when an entity with a default value
    /// of the property is added to the context.
    /// </summary>
    /// <param name="builder">The property builder.</param>
    /// <param name="type">Type of the generated TypeIds.</param>
    /// <exception cref="FormatException">The type is not a valid TypeId type.</exception>
    public static PropertyBuilder<TypeId> HasTypeIdGenerator(this PropertyBuilder<TypeId> builder, string type)
    {
        var generator = new TypeIdValueGenerator(type);
        return builder.HasValueGenerator((_, _) => generator).ValueGeneratedOnAdd();
    }

    /// <inheritdoc cref="HasTypeIdGenerator(PropertyBuilder{TypeId}, string)"/>
    public static PropertyBuilder<TypeIdDecoded> HasTypeIdGenerator(this PropertyBuilder<TypeIdDecoded> builder, string type)
    {
        var generator = new TypeIdDecodedValueGenerator(type);
        return builder.HasValueGenerator((_, _) => generator).ValueGeneratedOnAdd();
    }
}
