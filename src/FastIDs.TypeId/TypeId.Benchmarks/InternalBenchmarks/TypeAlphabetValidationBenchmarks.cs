using System.Buffers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.InternalBenchmarks;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
[MarkdownExporterAttribute.GitHub]
public class TypeAlphabetValidationBenchmarks
{
    [Params(3, 6, 8, 14, 30, 63)]
    public int PrefixLength;
    
    private string[] _prefixes = [];
    
    private const string AlphabetStr = "abcdefghijklmnopqrstuvwxyz";
    private readonly SearchValues<char> _searchValues = SearchValues.Create(AlphabetStr);
    private const int UnrollValue = 4;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random();
        
        const int count = 100_000;
        _prefixes = new string[count];
        var sb = new StringBuilder(PrefixLength);
        
        for (var i = 0; i < count; i++)
        {
            sb.Clear();
            
            for (var j = 0; j < PrefixLength; j++)
            {
                sb.Append((char)random.Next('a', 'z'));
            }

            _prefixes[i] = sb.ToString();
        }
    }
    
    [Benchmark(Baseline = true)]
    public bool CharCheck()
    {
        var isValid = false;
        foreach (var prefix in _prefixes)
        {
            foreach (var c in prefix.AsSpan())
            {
                isValid &= c is >= 'a' and <= 'z';
            }
        }

        return isValid;
    }
    
    [Benchmark]
    public bool CharCheckAscii()
    {
        var isValid = false;
        foreach (var prefix in _prefixes)
        {
            foreach (var c in prefix.AsSpan())
            {
                isValid &= char.IsAsciiLetterLower(c);
            }
        }

        return isValid;
    }

    [Benchmark]
    public bool SearchValuesCheck()
    {
        var isValid = false;
        foreach (var prefix in _prefixes)
        {
            foreach (var c in prefix.AsSpan())
            {
                isValid &= _searchValues.Contains(c);
            }
        }
    
        return isValid;
    }
    
    [Benchmark]
    public bool SearchValuesInRangeCheck()
    {
        var isValid = false;
        foreach (var prefix in _prefixes)
        {
            isValid &= !prefix.AsSpan().ContainsAnyExceptInRange('a', 'z');
        }
    
        return isValid;
    }

    [Benchmark]
    public bool Simd()
    {
        var isValid = false;
        foreach (var prefix in _prefixes)
        {
            var lower = new Vector<short>((short)'a');
            var higher = new Vector<short>((short)'z');
    
            var shorts = MemoryMarshal.Cast<char, short>(prefix.AsSpan());
            
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
}