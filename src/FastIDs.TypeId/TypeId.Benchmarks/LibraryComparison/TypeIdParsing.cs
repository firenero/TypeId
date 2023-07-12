using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
public class TypeIdParsing
{
    [Params(10, 1000, 1_000_000, 1_000_000_000)]
    public int Iterations;

    private string[] _typeIdStrings;

    [GlobalSetup]
    public void Setup()
    {
        _typeIdStrings = new string[Iterations];
        for (var i = 0; i < Iterations; i++)
        {
            _typeIdStrings[i] = TypeId.New("prefix").ToString();
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
    public global::TypeId.TypeId EvgregParse()
    {
        global::TypeId.TypeId typeId = default;

        foreach (var str in _typeIdStrings)
        {
            typeId = global::TypeId.TypeId.Parse(str);
        }

        return typeId;
    }
    
    [Benchmark]
    public global::TypeId.TypeId EvgregTryParse()
    {
        global::TypeId.TypeId typeId = default;

        foreach (var str in _typeIdStrings)
        {
            global::TypeId.TypeId.TryParse(str, out typeId);
        }

        return typeId;
    }
}