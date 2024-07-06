using System.Runtime.CompilerServices;

namespace FastIDs.TypeId.Uuid;

// The UUIDv7 implementation is extracted from https://github.com/mareek/UUIDNext to prevent transient dependency.
// TypeID doesn't require any UUID implementations except UUIDv7.
internal static class GuidConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid CreateGuidFromBigEndianBytes(Span<byte> bytes)
    {
        SetVersion(bytes);
        SetVariant(bytes);
        return new Guid(bytes, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetVersion(Span<byte> bytes)
    {
        const byte uuidVersion = 7;
        const int versionByteIndex = 6;
        //Erase upper 4 bits
        bytes[versionByteIndex] &= 0b0000_1111;
        //Set 4 upper bits to version
        bytes[versionByteIndex] |= uuidVersion << 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetVariant(Span<byte> bytes)
    {
        const int variantByteIndex = 8;
        //Erase upper 2 bits
        bytes[variantByteIndex] &= 0b0011_1111;
        //Set 2 upper bits to variant
        bytes[variantByteIndex] |= 0b1000_0000;
    }
}