using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.GitHub]
[MarkdownExporterAttribute.Default]
public class TypeIdParsing
{
    [Params(1_000_000)]
    public int Iterations;
    
    [Params(5, 10, 63)]
    public int PrefixLength;

    private string[] _typeIdStrings = Array.Empty<string>();

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        var sb = new StringBuilder(PrefixLength);
        for (var i = 0; i < PrefixLength; i++)
        {
            var letter = (char) random.Next('a', 'z');
            sb.Append(letter);
        }
        var prefix = PrefixLength > 0 ? sb.ToString() : "";
        
        _typeIdStrings = new string[Iterations];
        for (var i = 0; i < Iterations; i++)
        {
            _typeIdStrings[i] = TypeId.New(prefix).ToString();
        }
    }

    [Benchmark(Baseline = true)]
    public TypeId FastIdsParse()
    {
        TypeId typeId = default;
        foreach (var str in _typeIdStrings)
        {
            typeId = TypeId.Parse(str);
        }

        return typeId;
    }
    
    [Benchmark]
    public TypeId FastIdsTryParse()
    {
        TypeId typeId = default;
        foreach (var str in _typeIdStrings)
        {
            TypeId.TryParse(str, out typeId);
        }

        return typeId;
    }

    [Benchmark]
    public TcKs.TypeId.TypeId TcKsParse()
    {
        TcKs.TypeId.TypeId typeId = default;
        foreach (var str in _typeIdStrings)
        {
            typeId = TcKs.TypeId.TypeId.Parse(str);
        }
        
        return typeId;
    }
    
    [Benchmark]
    public TcKs.TypeId.TypeId TcKsTryParse()
    {
        TcKs.TypeId.TypeId typeId = default;
        foreach (var str in _typeIdStrings)
        {
            TcKs.TypeId.TypeId.TryParse(str, out typeId);
        }
        
        return typeId;
    }

    [Benchmark]
    public global::TypeId.TypeId CbuctokParse()
    {
        global::TypeId.TypeId typeId = default;

        foreach (var str in _typeIdStrings)
        {
            typeId = global::TypeId.TypeId.Parse(str);
        }

        return typeId;
    }
    
    [Benchmark]
    public global::TypeId.TypeId CbuctokTryParse()
    {
        global::TypeId.TypeId typeId = default;

        foreach (var str in _typeIdStrings)
        {
            global::TypeId.TypeId.TryParse(str, out typeId);
        }

        return typeId;
    }
}