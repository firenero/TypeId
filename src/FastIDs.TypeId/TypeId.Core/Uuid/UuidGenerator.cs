using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace FastIDs.TypeId.Uuid;

// The UUIDv7 implementation is extracted from https://github.com/mareek/UUIDNext to prevent transient dependency.
// TypeID doesn't require any UUID implementations except UUIDv7.

// All timestamps of type `long` in this class are Unix milliseconds unless stated otherwise.
internal sealed class UuidGenerator
{
    private const int SequenceBitSize = 7;
    private const int SequenceMaxValue = (1 << SequenceBitSize) - 1;
    
    private long _lastUsedTimestamp;
    private long _timestampOffset;
    private ushort _monotonicSequence;

    public Guid New()
    {
        // Allocating 2 bytes more to prepend timestamp data.
        Span<byte> buffer = stackalloc byte[18];
        
        // Offset bytes that are used in ID.
        var idBytes = buffer[2..];

        var timestamp = GetCurrentUnixMilliseconds();
        SetSequence(idBytes[6..8], ref timestamp);
        SetTimestamp(buffer[..8], timestamp); // Using full buffer because we need to account for two zero-bytes in front.
        RandomNumberGenerator.Fill(idBytes[8..]);

        return GuidConverter.CreateGuidFromBigEndianBytes(idBytes);
    }

    // The implementation copied from DateTimeOffset.ToUnixTimeMilliseconds()
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetCurrentUnixMilliseconds() => DateTime.UtcNow.Ticks / 10000L - 62135596800000L;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetTimestamp(Span<byte> bytes, long unixMs)
    {
        BinaryPrimitives.TryWriteInt64BigEndian(bytes, unixMs);
    }

    private void SetSequence(Span<byte> bytes, ref long timestamp)
    {
        ushort sequence;
        var originalTimestamp = timestamp;

        lock (this)
        {
            sequence = GetSequenceNumber(ref timestamp);
            if (sequence > SequenceMaxValue)
            {
                // if the sequence is greater than the max value, we take advantage
                // of the anti-rewind mechanism to simulate a slight change in clock time
                timestamp = originalTimestamp + 1;
                sequence = GetSequenceNumber(ref timestamp);
            }
        }

        BinaryPrimitives.TryWriteUInt16BigEndian(bytes, sequence);
    }
    
    private ushort GetSequenceNumber(ref long timestamp)
    {
        EnsureTimestampNeverMoveBackward(ref timestamp);

        if (timestamp == _lastUsedTimestamp)
        {
            _monotonicSequence += 1;
        }
        else
        {
            _lastUsedTimestamp = timestamp;
            _monotonicSequence = GetSequenceSeed();
        }

        return _monotonicSequence;
    }
    
    private void EnsureTimestampNeverMoveBackward(ref long timestamp)
    {
        var lastUsedTs = _lastUsedTimestamp;
        if (_timestampOffset > 0 && timestamp > lastUsedTs)
        {
            // reset the offset to reduce the drift with the actual time when possible
            _timestampOffset = 0;
            return;
        }

        var offsetTimestamp = timestamp + _timestampOffset;
        if (offsetTimestamp < lastUsedTs)
        {
            // if the computer clock has moved backward since the last generated UUID,
            // we add an offset to ensure the timestamp always move forward (See RFC Section 6.2)
            _timestampOffset = lastUsedTs - timestamp;
            timestamp = lastUsedTs;
            return;
        }

        // Happy path
        timestamp = offsetTimestamp;
    }


    private static ushort GetSequenceSeed()
    {
        // following section 6.2 on "Fixed-Length Dedicated Counter Seeding", the initial value of the sequence is randomized
        Span<byte> buffer = stackalloc byte[2];
        RandomNumberGenerator.Fill(buffer);
        // Setting the highest bit to 0 mitigate the risk of a sequence overflow (see section 6.2)
        buffer[0] &= 0b0000_0111;
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }
}