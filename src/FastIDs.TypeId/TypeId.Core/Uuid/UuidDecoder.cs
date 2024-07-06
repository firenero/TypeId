using System.Buffers.Binary;

namespace FastIDs.TypeId.Uuid;

// The UUIDv7 implementation is extracted from https://github.com/mareek/UUIDNext to prevent transient dependency.
// TypeID doesn't require any UUID implementations except UUIDv7.
internal static class UuidDecoder
{
    public static (long timestampMs, short sequence) Decode(Guid guid)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes, bigEndian: true, out _);

        Span<byte> timestampBytes = stackalloc byte[8];
        bytes[..6].CopyTo(timestampBytes[2..]);
        var timestampMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

        var sequenceBytes = bytes[6..8];
        //remove version information
        sequenceBytes[0] &= 0b0000_1111;
        var sequence = BinaryPrimitives.ReadInt16BigEndian(sequenceBytes);

        return (timestampMs, sequence);
    }
}