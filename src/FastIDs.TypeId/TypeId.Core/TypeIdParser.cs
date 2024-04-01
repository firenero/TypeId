using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

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
        // Vectorized version is faster for strings with length >= 8.
        const int vectorizedThreshold = 8;
        if (Vector128.IsHardwareAccelerated && type.Length >= vectorizedThreshold)
            return !type.ContainsAnyExceptInRange('a', 'z');
        
        // Fallback to scalar version for strings with length < 8 or when hardware intrinsics are not available.
        foreach (var c in type)
        {
            if (!char.IsAsciiLetterLower(c))
                return false;
        }

        return true;
    }
}