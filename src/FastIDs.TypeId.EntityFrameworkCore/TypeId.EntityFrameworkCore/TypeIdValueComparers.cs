using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FastIDs.TypeId.EntityFrameworkCore;

/// <summary>
/// Compares <see cref="TypeId"/> values for EF Core change tracking, identity maps and key lookups.
/// </summary>
/// <remarks>
/// <c>default(TypeId)</c> has no underlying string and <see cref="TypeId.GetHashCode"/> can't hash it, but EF Core
/// hashes default values all the time (e.g. a foreign key that is not populated yet when an entity graph is attached).
/// This comparer treats the default value as a regular value with hash code 0.
/// The configuration methods of this package register it automatically.
/// </remarks>
public sealed class TypeIdValueComparer : ValueComparer<TypeId>
{
    /// <summary>
    /// Creates the comparer.
    /// </summary>
    public TypeIdValueComparer()
        : base(
            (left, right) => left.Equals(right),
            value => TypeIdConversions.GetHashCode(value),
            value => value)
    {
    }
}

/// <summary>
/// Compares <see cref="TypeIdDecoded"/> values for EF Core change tracking, identity maps and key lookups.
/// </summary>
/// <remarks>
/// <c>default(TypeIdDecoded)</c> is a regular value for this comparer. Both components are immutable, so no
/// snapshot copy is needed.
/// </remarks>
public sealed class TypeIdDecodedValueComparer : ValueComparer<TypeIdDecoded>
{
    /// <summary>
    /// Creates the comparer.
    /// </summary>
    public TypeIdDecodedValueComparer()
        : base(
            (left, right) => left.Equals(right),
            value => value.GetHashCode(),
            value => value)
    {
    }
}
