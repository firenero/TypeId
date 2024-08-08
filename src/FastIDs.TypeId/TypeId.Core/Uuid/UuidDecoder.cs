using System.Buffers.Binary;

namespace FastIDs.TypeId.Uuid;

// The UUIDv7 implementation is extracted from https://github.com/mareek/UUIDNext to prevent transient dependency.
// TypeID doesn't require any UUID implementations except UUIDv7.
internal static class UuidDecoder
{
    public static long DecodeTimestamp(Guid guid)
    {
        // Allocating 2 bytes more to prepend timestamp data.
        Span<byte> bytes = stackalloc byte[18];
        guid.TryWriteBytes(bytes[2..], bigEndian: true, out _);

        var timestampBytes = bytes[..8];
        var timestampMs = BinaryPrimitives.ReadInt64BigEndian(timestampBytes);

        return timestampMs;
    }

}