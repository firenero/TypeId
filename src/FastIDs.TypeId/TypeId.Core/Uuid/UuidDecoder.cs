namespace FastIDs.TypeId.Uuid;

internal static class UuidDecoder
{
    public static long DecodeTimestamp(Guid guid)
    {
        // The 48-bit timestamp is the top 48 bits of the big-endian UUID.
        Base32.GetBigEndianBits(guid, out var hi, out _);
        return (long)(hi >> 16);
    }
}
