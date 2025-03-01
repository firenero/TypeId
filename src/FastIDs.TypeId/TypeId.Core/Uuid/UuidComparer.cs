namespace FastIDs.TypeId.Uuid;

/// <summary>
/// Compares two big endian <see cref="Guid"/>
/// </summary>
internal class UuidComparer : IComparer<Guid>
{
    public static UuidComparer Instance { get; } = new();
    
    public int Compare(Guid x, Guid y)
    {
        const int bytesCount = 16;
        Span<byte> xBytes = stackalloc byte[bytesCount];
        x.TryWriteBytes(xBytes, bigEndian: true, out _);

        Span<byte> yBytes = stackalloc byte[bytesCount];
        y.TryWriteBytes(yBytes, bigEndian: true, out _);

        for (var i = 0; i < bytesCount; i++)
        {
            var compareResult = xBytes[i].CompareTo(yBytes[i]);
            if (compareResult != 0)
                return compareResult;
        }

        return 0;
    }
}