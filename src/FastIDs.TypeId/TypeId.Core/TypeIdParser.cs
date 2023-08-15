using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FastIDs.TypeId;

internal static class TypeIdParser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FormatUuidBytes(Span<byte> bytes)
    {
        // Optimized version of:
        // (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
        // (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
        var num = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(bytes[..4]));
        num = BinaryPrimitives.ReverseEndianness(num);
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(bytes[..4]), num);
        
        // Using permutations here because benchmarks show that they are faster for uint16.
        (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
        (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ValidateTypeAlphabet(ReadOnlySpan<char> type)
    {
        return Vector.IsHardwareAccelerated && type.Length >= Vector<ushort>.Count
            ? ValidateTypeAlphabetVectorized(type)
            : ValidateTypeAlphabetSequential(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ValidateTypeAlphabetSequential(ReadOnlySpan<char> type)
    {
        foreach (var c in type)
        {
            if (c is < 'a' or > 'z')
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ValidateTypeAlphabetVectorized(ReadOnlySpan<char> type)
    {
        var lower = new Vector<ushort>('a');
        var upper = new Vector<ushort>('z');

        var shorts = MemoryMarshal.Cast<char, ushort>(type);
        for (var i = 0; i < shorts.Length; i += Vector<ushort>.Count)
        {
            var span = Vector<ushort>.Count < shorts.Length - i
                ? shorts.Slice(i, Vector<ushort>.Count)
                : shorts[^Vector<ushort>.Count..];
            
            var curVector = new Vector<ushort>(span);
            
            var isGreater = Vector.GreaterThanOrEqualAll(curVector, lower);
            if (!isGreater)
                return false;
            
            var isLower = Vector.LessThanOrEqualAll(curVector, upper);
            if (!isLower)
                return false;
        }
        
        return true;
    }
}