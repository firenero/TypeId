using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace FastIDs.TypeId;

internal static class TypeIdParser
{
    private static readonly SearchValues<char> Alphabet = SearchValues.Create("_abcdefghijklmnopqrstuvwxyz");
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

    public static TypeError ValidateType(ReadOnlySpan<char> type)
    {
        if (type.Length == 0)
            return TypeError.None;
        
        if (type[0] == '_')
            return TypeError.StartsWithUnderscore;
        if (type[^1] == '_')
            return TypeError.EndsWithUnderscore;
        
        // Vectorized version is faster for strings with length >= 8.
        const int vectorizedThreshold = 8;
        if (Vector128.IsHardwareAccelerated && type.Length >= vectorizedThreshold)
            return type.ContainsAnyExcept(Alphabet) ? TypeError.InvalidChar : TypeError.None;
        
        // Fallback to scalar version for strings with length < 8 or when hardware intrinsics are not available.
        foreach (var c in type)
        {
            var isValidChar = c is >= 'a' and <= 'z' or '_';
            if (!isValidChar)
                return TypeError.InvalidChar;
        }

        return TypeError.None;
    }

    public enum TypeError
    {
        None,
        StartsWithUnderscore,
        EndsWithUnderscore,
        InvalidChar,
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToErrorMessage(this TypeError error) => error switch
    {
        TypeError.None => "",
        TypeError.StartsWithUnderscore => "Type can't start with an underscore.",
        TypeError.EndsWithUnderscore => "Type can't end with an underscore.",
        TypeError.InvalidChar => "Type must contain only lowercase letters and underscores.",
        _ => "Unknown type error."
    };
}