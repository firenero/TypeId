using System.Buffers.Binary;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.InternalBenchmarks;

[MemoryDiagnoser]
public class FormatUuidBenchmarks
{
    private Guid _guid = new Guid("01890a5d-ac96-774b-bcce-b302099a8057");

    [Benchmark(Baseline = true)]
    public int SpanReverse()
    {
        Span<byte> bytes = stackalloc byte[16];
        _guid.TryWriteBytes(bytes);

        for (var i = 0; i < 1_000_000; i++)
        {
            bytes[..4].Reverse();
            (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
            (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
        }

        return bytes[3] + bytes[5];
    }

    [Benchmark]
    public int SpanSwap()
    {
        Span<byte> bytes = stackalloc byte[16];
        _guid.TryWriteBytes(bytes);

        for (var i = 0; i < 1_000_000; i++)
        {
            (bytes[0], bytes[3]) = (bytes[3], bytes[0]);
            (bytes[1], bytes[2]) = (bytes[2], bytes[1]);
            (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
            (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
        }

        return bytes[3] + bytes[5];
    }
    
    [Benchmark]
    public int SpanPrimitives()
    {
        Span<byte> bytes = stackalloc byte[16];
        _guid.TryWriteBytes(bytes);

        for (var i = 0; i < 1_000_000; i++)
        {
            var a = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes[..4]));
            BitConverter.TryWriteBytes(bytes[..4], a);

            (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
            (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
        }

        return bytes[3] + bytes[5];
    }
    
    [Benchmark]
    public int SpanPrimitivesFull()
    {
        Span<byte> bytes = stackalloc byte[16];
        _guid.TryWriteBytes(bytes);

        for (var i = 0; i < 1_000_000; i++)
        {
            var a = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(bytes[..4]));
            BitConverter.TryWriteBytes(bytes[..4], a);

            var b = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(bytes[4..6]));
            BitConverter.TryWriteBytes(bytes[4..6], b);

            var c = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(bytes[6..8]));
            BitConverter.TryWriteBytes(bytes[6..8], c);
        }

        return bytes[3] + bytes[5];
    }
}