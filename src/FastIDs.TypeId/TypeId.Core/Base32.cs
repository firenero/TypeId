using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FastIDs.TypeId;

internal static class Base32
{
    public static int Encode(in Guid id, Span<char> output)
    {
        if (output.Length < Base32Constants.EncodedLength)
            ThrowOutputTooSmall();

        GetBigEndianBits(id, out var hi, out var lo);
        EncodeImpl(hi, lo, ref MemoryMarshal.GetReference(output), ref MemoryMarshal.GetReference(Base32Constants.Alphabet.AsSpan()));
        return Base32Constants.EncodedLength;
    }

    public static int Encode(in Guid id, Span<byte> utf8Output)
    {
        if (utf8Output.Length < Base32Constants.EncodedLength)
            ThrowOutputTooSmall();

        GetBigEndianBits(id, out var hi, out var lo);
        EncodeImpl(hi, lo, ref MemoryMarshal.GetReference(utf8Output), ref MemoryMarshal.GetReference(Base32Constants.Utf8Alphabet));
        return Base32Constants.EncodedLength;
    }

    private static void ThrowOutputTooSmall() =>
        throw new FormatException($"Output must be at least {Base32Constants.EncodedLength} chars long.");

    /// <summary>
    /// Reads the UUID as a 128-bit big-endian (RFC 9562 byte order) number split into two halves.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetBigEndianBits(in Guid id, out ulong hi, out ulong lo)
    {
        // Guid layout: int _a; short _b; short _c; byte _d.._k.
        // _a, _b, _c are stored in native endianness, _d.._k are stored in RFC order.
        // Two 8-byte loads only: narrower loads at offsets 4/6 can miss store-to-load forwarding
        // when the Guid was just copied with a 16-byte vector store (measurable stall on AMD Zen).
        ref var p = ref Unsafe.As<Guid, byte>(ref Unsafe.AsRef(in id));
        var q0 = Unsafe.ReadUnaligned<ulong>(ref p);
        var q1 = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref p, 8));
        if (BitConverter.IsLittleEndian)
        {
            // q0 = _a | _b << 32 | _c << 48  ->  hi = _a << 32 | _b << 16 | _c
            hi = (q0 << 32) | ((q0 >> 16) & 0xFFFF_0000) | (q0 >> 48);
            lo = BinaryPrimitives.ReverseEndianness(q1);
        }
        else
        {
            hi = q0;
            lo = q1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Guid FromBigEndianBits(ulong hi, ulong lo) =>
        new((uint)(hi >> 32), (ushort)(hi >> 16), (ushort)hi,
            (byte)(lo >> 56), (byte)(lo >> 48), (byte)(lo >> 40), (byte)(lo >> 32),
            (byte)(lo >> 24), (byte)(lo >> 16), (byte)(lo >> 8), (byte)lo);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EncodeImpl<TChar>(ulong hi, ulong lo, ref TChar o, ref TChar alpha) where TChar : unmanaged
    {
        // alpha points to a 32-entry alphabet and every index below is masked to 0..31.
        // Note: avoid generic-math conversions (TChar.CreateTruncating) here - they don't get inlined in a method this size.

        // 128 bits are encoded as 26 chars: first char holds 3 bits, the rest hold 5 bits each.
        o = Unsafe.Add(ref alpha, (int)(hi >> 61));
        Unsafe.Add(ref o, 1) = Unsafe.Add(ref alpha, (int)(hi >> 56) & 31);
        Unsafe.Add(ref o, 2) = Unsafe.Add(ref alpha, (int)(hi >> 51) & 31);
        Unsafe.Add(ref o, 3) = Unsafe.Add(ref alpha, (int)(hi >> 46) & 31);
        Unsafe.Add(ref o, 4) = Unsafe.Add(ref alpha, (int)(hi >> 41) & 31);
        Unsafe.Add(ref o, 5) = Unsafe.Add(ref alpha, (int)(hi >> 36) & 31);
        Unsafe.Add(ref o, 6) = Unsafe.Add(ref alpha, (int)(hi >> 31) & 31);
        Unsafe.Add(ref o, 7) = Unsafe.Add(ref alpha, (int)(hi >> 26) & 31);
        Unsafe.Add(ref o, 8) = Unsafe.Add(ref alpha, (int)(hi >> 21) & 31);
        Unsafe.Add(ref o, 9) = Unsafe.Add(ref alpha, (int)(hi >> 16) & 31);
        Unsafe.Add(ref o, 10) = Unsafe.Add(ref alpha, (int)(hi >> 11) & 31);
        Unsafe.Add(ref o, 11) = Unsafe.Add(ref alpha, (int)(hi >> 6) & 31);
        Unsafe.Add(ref o, 12) = Unsafe.Add(ref alpha, (int)(hi >> 1) & 31);
        Unsafe.Add(ref o, 13) = Unsafe.Add(ref alpha, (int)((hi << 4) | (lo >> 60)) & 31);
        Unsafe.Add(ref o, 14) = Unsafe.Add(ref alpha, (int)(lo >> 55) & 31);
        Unsafe.Add(ref o, 15) = Unsafe.Add(ref alpha, (int)(lo >> 50) & 31);
        Unsafe.Add(ref o, 16) = Unsafe.Add(ref alpha, (int)(lo >> 45) & 31);
        Unsafe.Add(ref o, 17) = Unsafe.Add(ref alpha, (int)(lo >> 40) & 31);
        Unsafe.Add(ref o, 18) = Unsafe.Add(ref alpha, (int)(lo >> 35) & 31);
        Unsafe.Add(ref o, 19) = Unsafe.Add(ref alpha, (int)(lo >> 30) & 31);
        Unsafe.Add(ref o, 20) = Unsafe.Add(ref alpha, (int)(lo >> 25) & 31);
        Unsafe.Add(ref o, 21) = Unsafe.Add(ref alpha, (int)(lo >> 20) & 31);
        Unsafe.Add(ref o, 22) = Unsafe.Add(ref alpha, (int)(lo >> 15) & 31);
        Unsafe.Add(ref o, 23) = Unsafe.Add(ref alpha, (int)(lo >> 10) & 31);
        Unsafe.Add(ref o, 24) = Unsafe.Add(ref alpha, (int)(lo >> 5) & 31);
        Unsafe.Add(ref o, 25) = Unsafe.Add(ref alpha, (int)lo & 31);
    }

    /// <summary>
    /// Decodes an already validated suffix (see <see cref="IsValid"/>) into a UUID.
    /// </summary>
    public static Guid DecodeValidated(ReadOnlySpan<char> input)
    {
        if (input.Length != Base32Constants.EncodedLength)
            ThrowOutputTooSmall();

        ref var s = ref MemoryMarshal.GetReference(input);
        var d13 = Dec(ref s, 13);
        var hi = (Dec(ref s, 0) << 61) | (Dec(ref s, 1) << 56) | (Dec(ref s, 2) << 51) | (Dec(ref s, 3) << 46)
                 | (Dec(ref s, 4) << 41) | (Dec(ref s, 5) << 36) | (Dec(ref s, 6) << 31) | (Dec(ref s, 7) << 26)
                 | (Dec(ref s, 8) << 21) | (Dec(ref s, 9) << 16) | (Dec(ref s, 10) << 11) | (Dec(ref s, 11) << 6)
                 | (Dec(ref s, 12) << 1) | (d13 >> 4);
        var lo = (d13 << 60) | (Dec(ref s, 14) << 55) | (Dec(ref s, 15) << 50) | (Dec(ref s, 16) << 45)
                 | (Dec(ref s, 17) << 40) | (Dec(ref s, 18) << 35) | (Dec(ref s, 19) << 30) | (Dec(ref s, 20) << 25)
                 | (Dec(ref s, 21) << 20) | (Dec(ref s, 22) << 15) | (Dec(ref s, 23) << 10) | (Dec(ref s, 24) << 5)
                 | Dec(ref s, 25);

        return FromBigEndianBits(hi, lo);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Dec(ref char s, int i) => Base32Constants.DecodingTable[(byte)Unsafe.Add(ref s, i)];

    public static bool IsValid(ReadOnlySpan<char> input)
    {
        if (input.Length != Base32Constants.EncodedLength)
            return false;

        return !input.ContainsAnyExcept(Base32Constants.AlphabetValues);
    }
}
