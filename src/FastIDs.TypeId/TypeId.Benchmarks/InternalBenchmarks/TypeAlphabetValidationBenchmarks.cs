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
    
    private const string AlphabetStr = "_abcdefghijklmnopqrstuvwxyz";
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
                if (j == PrefixLength / 2)
                    sb.Append('_');
                else
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
                isValid &= c is '_' or >= 'a' and <= 'z';
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
                isValid &= char.IsAsciiLetterLower(c) || c == '_';
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
    public bool SearchValuesContainsAny()
    {
        var isValid = false;
        foreach (var prefix in _prefixes)
        {
            isValid &= prefix.AsSpan().ContainsAnyExcept(_searchValues);
        }
    
        return isValid;
    }
    
    [Benchmark]
    public bool InRangeCheck()
    {
        var isValid = false;
        foreach (var prefix in _prefixes)
        {
            var span = prefix.AsSpan();
            isValid &= !span.ContainsAnyExceptInRange('_', 'z') && !span.Contains('`');
        }
    
        return isValid;
    }
}