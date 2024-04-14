using System.Buffers;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.InternalBenchmarks;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
[MarkdownExporterAttribute.GitHub]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
public class IdAlphabetValidationBenchmarks
{
    private const string AlphabetStr = "0123456789abcdefghjkmnpqrstvwxyz";
    
    private readonly SearchValues<char> _searchValues = SearchValues.Create(AlphabetStr);
    private readonly HashSet<char> _alphabetSet = new(AlphabetStr);

    private string[] _validIds = [];
    private string[] _invalidIds = [];

    [GlobalSetup]
    public void Setup()
    {
        const int idsCount = 100_000;
        _validIds = new string[idsCount];
        _invalidIds = new string[idsCount];
        for (var i = 0; i < idsCount; i++)
        {
            var id = TypeId.New("", false).ToString();
            _validIds[i] = id;
            _invalidIds[i] = id[..^1] + ','; // invalid char

        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Valid")]
    public bool CharCheckUnrolledValid()
    {
        var isValid = false;
        foreach (var id in _validIds)
        {
            isValid &= IsValidAlphabet(id);
        }

        return isValid;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Invalid")]
    public bool CharCheckUnrolledInvalid()
    {
        var isValid = false;
        foreach (var id in _invalidIds)
        {
            isValid &= IsValidAlphabet(id);
        }

        return isValid;
    }

    [Benchmark]
    [BenchmarkCategory("Valid")]
    public bool SearchValuesLoopValid()
    {
        var isValid = false;
        foreach (var id in _validIds)
        {
            isValid &= IsValidAlphabetSearchValues(id);
        }

        return isValid;
    }
    
    [Benchmark]
    [BenchmarkCategory("Valid")]
    public bool SearchValuesUnrollValid()
    {
        var isValid = false;
        foreach (var id in _validIds)
        {
            isValid &= IsValidAlphabetSearchValuesUnrolled(id);
        }

        return isValid;
    }
    
    [Benchmark]
    [BenchmarkCategory("Invalid")]
    public bool SearchValuesLoopInvalid()
    {
        var isValid = false;
        foreach (var id in _invalidIds)
        {
            isValid &= IsValidAlphabetSearchValues(id);
        }

        return isValid;
    }
    
    [Benchmark]
    [BenchmarkCategory("Invalid")]
    public bool SearchValuesUnrollInvalid()
    {
        var isValid = false;
        foreach (var id in _invalidIds)
        {
            isValid &= IsValidAlphabetSearchValuesUnrolled(id);
        }

        return isValid;
    }
    
    [Benchmark]
    [BenchmarkCategory("Valid")]
    public bool HashSetLoopValid()
    {
        var isValid = false;
        foreach (var id in _validIds)
        {
            isValid &= IsValidAlphabetHashSet(id);
        }

        return isValid;
    }
    
    [Benchmark]
    [BenchmarkCategory("Valid")]
    public bool HashSetUnrollValid()
    {
        var isValid = false;
        foreach (var id in _validIds)
        {
            isValid &= IsValidAlphabetHashSetUnrolled(id);
        }

        return isValid;
    }
    
    [Benchmark]
    [BenchmarkCategory("Invalid")]
    public bool HashSetLoopInvalid()
    {
        var isValid = false;
        foreach (var id in _invalidIds)
        {
            isValid &= IsValidAlphabetHashSet(id);
        }

        return isValid;
    }
    
    [Benchmark]
    [BenchmarkCategory("Invalid")]
    public bool HashSetUnrollInvalid()
    {
        var isValid = false;
        foreach (var id in _invalidIds)
        {
            isValid &= IsValidAlphabetHashSetUnrolled(id);
        }

        return isValid;
    }

    private static bool IsValidAlphabet(ReadOnlySpan<char> chars) =>
        IsValidChar(chars[0])
        && IsValidChar(chars[1])
        && IsValidChar(chars[2])
        && IsValidChar(chars[3])
        && IsValidChar(chars[4])
        && IsValidChar(chars[5])
        && IsValidChar(chars[6])
        && IsValidChar(chars[7])
        && IsValidChar(chars[8])
        && IsValidChar(chars[9])
        && IsValidChar(chars[10])
        && IsValidChar(chars[11])
        && IsValidChar(chars[12])
        && IsValidChar(chars[13])
        && IsValidChar(chars[14])
        && IsValidChar(chars[15])
        && IsValidChar(chars[16])
        && IsValidChar(chars[17])
        && IsValidChar(chars[18])
        && IsValidChar(chars[19])
        && IsValidChar(chars[20])
        && IsValidChar(chars[21])
        && IsValidChar(chars[22])
        && IsValidChar(chars[23])
        && IsValidChar(chars[24])
        && IsValidChar(chars[25]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidChar(char c)
    {
        if (c >= '0' && c <= '9')
            return true;
        
        return c is >= 'a' and <= 'h'
            or >= 'j' and <= 'k'
            or >= 'm' and <= 'n'
            or >= 'p' and <= 't'
            or >= 'v' and <= 'z';
    }

    private bool IsValidAlphabetSearchValues(ReadOnlySpan<char> chars)
    {
        foreach (var c in chars)
        {
            if (!_searchValues.Contains(c))
                return false;
        }

        return true;
    }

    private bool IsValidAlphabetSearchValuesUnrolled(ReadOnlySpan<char> chars) =>
        _searchValues.Contains(chars[0])
        && _searchValues.Contains(chars[1])
        && _searchValues.Contains(chars[2])
        && _searchValues.Contains(chars[3])
        && _searchValues.Contains(chars[4])
        && _searchValues.Contains(chars[5])
        && _searchValues.Contains(chars[6])
        && _searchValues.Contains(chars[7])
        && _searchValues.Contains(chars[8])
        && _searchValues.Contains(chars[9])
        && _searchValues.Contains(chars[10])
        && _searchValues.Contains(chars[11])
        && _searchValues.Contains(chars[12])
        && _searchValues.Contains(chars[13])
        && _searchValues.Contains(chars[14])
        && _searchValues.Contains(chars[15])
        && _searchValues.Contains(chars[16])
        && _searchValues.Contains(chars[17])
        && _searchValues.Contains(chars[18])
        && _searchValues.Contains(chars[19])
        && _searchValues.Contains(chars[20])
        && _searchValues.Contains(chars[21])
        && _searchValues.Contains(chars[22])
        && _searchValues.Contains(chars[23])
        && _searchValues.Contains(chars[24])
        && _searchValues.Contains(chars[25]);

    private bool IsValidAlphabetHashSet(ReadOnlySpan<char> chars)
    {
        foreach (var c in chars)
        {
            if (!_alphabetSet.Contains(c))
                return false;
        }

        return true;
    }
    
    private bool IsValidAlphabetHashSetUnrolled(ReadOnlySpan<char> chars) =>
        _alphabetSet.Contains(chars[0])
        && _alphabetSet.Contains(chars[1])
        && _alphabetSet.Contains(chars[2])
        && _alphabetSet.Contains(chars[3])
        && _alphabetSet.Contains(chars[4])
        && _alphabetSet.Contains(chars[5])
        && _alphabetSet.Contains(chars[6])
        && _alphabetSet.Contains(chars[7])
        && _alphabetSet.Contains(chars[8])
        && _alphabetSet.Contains(chars[9])
        && _alphabetSet.Contains(chars[10])
        && _alphabetSet.Contains(chars[11])
        && _alphabetSet.Contains(chars[12])
        && _alphabetSet.Contains(chars[13])
        && _alphabetSet.Contains(chars[14])
        && _alphabetSet.Contains(chars[15])
        && _alphabetSet.Contains(chars[16])
        && _alphabetSet.Contains(chars[17])
        && _alphabetSet.Contains(chars[18])
        && _alphabetSet.Contains(chars[19])
        && _alphabetSet.Contains(chars[20])
        && _alphabetSet.Contains(chars[21])
        && _alphabetSet.Contains(chars[22])
        && _alphabetSet.Contains(chars[23])
        && _alphabetSet.Contains(chars[24])
        && _alphabetSet.Contains(chars[25]);
}