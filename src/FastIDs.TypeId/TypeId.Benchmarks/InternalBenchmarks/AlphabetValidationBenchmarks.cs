using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.InternalBenchmarks;

[MemoryDiagnoser]
public class AlphabetValidationBenchmarks
{
    [Params(3, 6, 10, 16, 30, 63)]
    public int PrefixLength;
    
    private string _prefix = "";
    
    private const string AlphabetStr = "abcdefghijklmnopqrstuvwxyz";
    private readonly HashSet<char> _alphabetSet = new(AlphabetStr);
    private const int LoopIterationsCount = 100_000;
    private const int UnrollValue = 4;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        _prefix = "";
        for (var i = 0; i < PrefixLength; i++)
        {
            _prefix += (char) random.Next('a', 'z');
        }
    }
    
    [Benchmark(Baseline = true)]
    public bool CharCheck()
    {
        var isValid = false;
        for (var i = 0; i < LoopIterationsCount; i++)
        {
            foreach (var c in _prefix)
            {
                isValid &= c is >= 'a' and <= 'z';
            }
        }

        return isValid;
    }
    
    [Benchmark]
    public bool Simd()
    {
        var isValid = false;
        for (var i = 0; i < LoopIterationsCount; i++)
        {
            var lower = new Vector<short>((short)'a');
            var higher = new Vector<short>((short)'z');

            var shorts = MemoryMarshal.Cast<char, short>(_prefix.AsSpan());
            
            for (var j = 0; j < shorts.Length; j += Vector<short>.Count)
            {
                var span = Vector<short>.Count < shorts.Length - j
                    ? shorts.Slice(j, Vector<short>.Count)
                    : shorts[^Vector<short>.Count..];

                var curVector = new Vector<short>(span);

                var isGreater = Vector.GreaterThanOrEqualAll(curVector, lower);
                var isLower = Vector.LessThanOrEqualAll(curVector, higher);
                isValid &= isGreater && isLower;
            }
        }

        return isValid;
    }
    
    [Benchmark]
    public bool CharCheckUnroll()
    {
        var isValid = false;
        for (var i = 0; i < LoopIterationsCount; i++)
        {
            if (_prefix.Length < UnrollValue)
            {
                foreach (var c in _prefix)
                {
                    isValid &= c is >= 'a' and <= 'z';
                }
            }
            else
            {
                var j = 0;
                ref var prefixStart = ref MemoryMarshal.GetReference(_prefix.AsSpan());
                
                // unroll loop
                for (; j + UnrollValue < _prefix.Length; j += UnrollValue)
                {
                    isValid &= Unsafe.Add(ref prefixStart, j) is >= 'a' and <= 'z';
                    isValid &= Unsafe.Add(ref prefixStart, j + 1) is >= 'a' and <= 'z';
                    isValid &= Unsafe.Add(ref prefixStart, j + 2) is >= 'a' and <= 'z';
                    isValid &= Unsafe.Add(ref prefixStart, j + 3) is >= 'a' and <= 'z';
                }
                
                for (; j < _prefix.Length; j++)
                {
                    isValid &= Unsafe.Add(ref prefixStart, j) is >= 'a' and <= 'z';
                }
            }
            
        }

        return isValid;
    }
}