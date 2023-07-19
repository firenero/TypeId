using System.Text;

namespace FastIDs.TypeId;

internal static class Base32
{
    public static int Encode(ReadOnlySpan<byte> bytes, Span<char> output)
    {
        if (bytes.Length != Base32Constants.DecodedLength)
            throw new FormatException($"Input must be {Base32Constants.DecodedLength} bytes long.");
        if (output.Length < Base32Constants.EncodedLength)
            throw new FormatException($"Output must be at least {Base32Constants.EncodedLength} chars long.");
        
        const string alpha = Base32Constants.Alphabet;
        // 10 byte timestamp
        output[0] = alpha[(bytes[0] & 224) >> 5];
        output[1] = alpha[bytes[0] & 31];
        output[2] = alpha[(bytes[1] & 248) >> 3];
        output[3] = alpha[((bytes[1] & 7) << 2) | ((bytes[2] & 192) >> 6)];
        output[4] = alpha[(bytes[2] & 62) >> 1];
        output[5] = alpha[((bytes[2] & 1) << 4) | ((bytes[3] & 240) >> 4)];
        output[6] = alpha[((bytes[3] & 15) << 1) | ((bytes[4] & 128) >> 7)];
        output[7] = alpha[(bytes[4] & 124) >> 2];
        output[8] = alpha[((bytes[4] & 3) << 3) | ((bytes[5] & 224) >> 5)];
        output[9] = alpha[bytes[5] & 31];

        // 16 bytes of entropy
        output[10] = alpha[(bytes[6] & 248) >> 3];
        output[11] = alpha[((bytes[6] & 7) << 2) | ((bytes[7] & 192) >> 6)];
        output[12] = alpha[(bytes[7] & 62) >> 1];
        output[13] = alpha[((bytes[7] & 1) << 4) | ((bytes[8] & 240) >> 4)];
        output[14] = alpha[((bytes[8] & 15) << 1) | ((bytes[9] & 128) >> 7)];
        output[15] = alpha[(bytes[9] & 124) >> 2];
        output[16] = alpha[((bytes[9] & 3) << 3) | ((bytes[10] & 224) >> 5)];
        output[17] = alpha[bytes[10] & 31];
        output[18] = alpha[(bytes[11] & 248) >> 3];
        output[19] = alpha[((bytes[11] & 7) << 2) | ((bytes[12] & 192) >> 6)];
        output[20] = alpha[(bytes[12] & 62) >> 1];
        output[21] = alpha[((bytes[12] & 1) << 4) | ((bytes[13] & 240) >> 4)];
        output[22] = alpha[((bytes[13] & 15) << 1) | ((bytes[14] & 128) >> 7)];
        output[23] = alpha[(bytes[14] & 124) >> 2];
        output[24] = alpha[((bytes[14] & 3) << 3) | ((bytes[15] & 224) >> 5)];
        output[25] = alpha[bytes[15] & 31];
        
        return Base32Constants.EncodedLength;
    }

    public static bool TryDecode(ReadOnlySpan<char> input, Span<byte> output)
    {
        if (input.Length != Base32Constants.EncodedLength)
            return false;

        Span<byte> inputBytes = stackalloc byte[Base32Constants.EncodedLength];
        var writtenBytesCount = Encoding.UTF8.GetBytes(input, inputBytes);
        if (writtenBytesCount != Base32Constants.EncodedLength)
            return false;

        if (!AreValidBytes(inputBytes))
            return false;

        var dec = Base32Constants.DecodingTable;
        // 6 bytes timestamp (48 bits)
        output[0] = (byte)((dec[inputBytes[0]] << 5) | dec[inputBytes[1]]);
        output[1] = (byte)((dec[inputBytes[2]] << 3) | (dec[inputBytes[3]] >> 2));
        output[2] = (byte)((dec[inputBytes[3]] << 6) | (dec[inputBytes[4]] << 1) | (dec[inputBytes[5]] >> 4));
        output[3] = (byte)((dec[inputBytes[5]] << 4) | (dec[inputBytes[6]] >> 1));
        output[4] = (byte)((dec[inputBytes[6]] << 7) | (dec[inputBytes[7]] << 2) | (dec[inputBytes[8]] >> 3));
        output[5] = (byte)((dec[inputBytes[8]] << 5) | dec[inputBytes[9]]);

        // 10 bytes of entropy (80 bits)
        output[6] = (byte)((dec[inputBytes[10]] << 3) | (dec[inputBytes[11]] >> 2)); // First 4 bits are the version
        output[7] = (byte)((dec[inputBytes[11]] << 6) | (dec[inputBytes[12]] << 1) | (dec[inputBytes[13]] >> 4));
        output[8] = (byte)((dec[inputBytes[13]] << 4) | (dec[inputBytes[14]] >> 1)); // First 2 bits are the variant
        output[9] = (byte)((dec[inputBytes[14]] << 7) | (dec[inputBytes[15]] << 2) | (dec[inputBytes[16]] >> 3));
        output[10] = (byte)((dec[inputBytes[16]] << 5) | dec[inputBytes[17]]);
        output[11] = (byte)((dec[inputBytes[18]] << 3) | dec[inputBytes[19]] >> 2);
        output[12] = (byte)((dec[inputBytes[19]] << 6) | (dec[inputBytes[20]] << 1) | (dec[inputBytes[21]] >> 4));
        output[13] = (byte)((dec[inputBytes[21]] << 4) | (dec[inputBytes[22]] >> 1));
        output[14] = (byte)((dec[inputBytes[22]] << 7) | (dec[inputBytes[23]] << 2) | (dec[inputBytes[24]] >> 3));
        output[15] = (byte)((dec[inputBytes[24]] << 5) | dec[inputBytes[25]]);

        return true;
    }
    
    public static bool IsValid(ReadOnlySpan<char> input)
    {
        if (input.Length != Base32Constants.EncodedLength)
            return false;

        Span<byte> inputBytes = stackalloc byte[Base32Constants.EncodedLength];
        var writtenBytesCount = Encoding.UTF8.GetBytes(input, inputBytes);
        if (writtenBytesCount != Base32Constants.EncodedLength)
            return false;

        return AreValidBytes(inputBytes);
    }

    private static bool AreValidBytes(ReadOnlySpan<byte> bytes)
    {
        var dec = Base32Constants.DecodingTable;
        return dec[bytes[0]] != 0xFF
               && dec[bytes[1]] != 0xFF
               && dec[bytes[2]] != 0xFF
               && dec[bytes[3]] != 0xFF
               && dec[bytes[4]] != 0xFF
               && dec[bytes[5]] != 0xFF
               && dec[bytes[6]] != 0xFF
               && dec[bytes[7]] != 0xFF
               && dec[bytes[8]] != 0xFF
               && dec[bytes[9]] != 0xFF
               && dec[bytes[10]] != 0xFF
               && dec[bytes[11]] != 0xFF
               && dec[bytes[12]] != 0xFF
               && dec[bytes[13]] != 0xFF
               && dec[bytes[14]] != 0xFF
               && dec[bytes[15]] != 0xFF
               && dec[bytes[16]] != 0xFF
               && dec[bytes[17]] != 0xFF
               && dec[bytes[18]] != 0xFF
               && dec[bytes[19]] != 0xFF
               && dec[bytes[20]] != 0xFF
               && dec[bytes[21]] != 0xFF
               && dec[bytes[22]] != 0xFF
               && dec[bytes[23]] != 0xFF
               && dec[bytes[24]] != 0xFF
               && dec[bytes[25]] != 0xFF;
    }
}