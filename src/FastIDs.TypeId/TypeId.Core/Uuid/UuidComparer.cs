namespace FastIDs.TypeId.Uuid;

/// <summary>
/// Compares two big endian <see cref="Guid"/>
/// </summary>
internal class UuidComparer : IComparer<Guid>
{
    public static UuidComparer Instance { get; } = new();

    public int Compare(Guid x, Guid y)
    {
        Base32.GetBigEndianBits(x, out var xHi, out var xLo);
        Base32.GetBigEndianBits(y, out var yHi, out var yLo);

        return xHi != yHi ? xHi.CompareTo(yHi) : xLo.CompareTo(yLo);
    }
}
