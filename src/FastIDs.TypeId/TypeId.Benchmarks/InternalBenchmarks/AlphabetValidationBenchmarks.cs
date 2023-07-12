using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.InternalBenchmarks;

[MemoryDiagnoser]
public class AlphabetValidationBenchmarks
{
    [Params(3, 6, 10, 30, 63)]
    public int PrefixLength;

    private string _prefix;
    
    private const string AlphabetStr = "abcdefghijklmnopqrstuvwxyz";
    private readonly HashSet<char> _alphabetSet = new(AlphabetStr);
    
    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        for (var i = 0; i < PrefixLength; i++)
        {
            _prefix += (char) random.Next('a', 'z');
        }
        _prefix = new string('a', PrefixLength);
    }
    
    [Benchmark]
    public bool AlphabetStringBenchmark()
    {
        var isValid = false;
        for (var i = 0; i < 1_000_000; i++)
        {
            foreach (var c in _prefix)
            {
                isValid = AlphabetStr.Contains(c);
            }    
        }

        return isValid;
    }
    
    [Benchmark]
    public bool AlphabetSetBenchmark()
    {
        var isValid = false;
        for (var i = 0; i < 1_000_000; i++)
        {
            foreach (var c in _prefix)
            {
                isValid = _alphabetSet.Contains(c);
            }    
        }

        return isValid;
    }
    
    [Benchmark(Baseline = true)]
    public bool CharCheckBenchmark()
    {
        var isValid = false;
        for (var i = 0; i < 1_000_000; i++)
        {
            foreach (var c in _prefix)
            {
                isValid = c is >= 'a' and <= 'z';
            }
        }

        return isValid;
    }
}