using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
public class TypeIdString
{
    [Params(10_000_000)]
    public int Iterations;
    
    private const string Prefix = "prefix";

    private TypeId[] _fastIdTypeIds;
    private TcKs.TypeId.TypeId[] _tcKsTypeIds;
    private global::TypeId.TypeId[] _evgregTypeIds;

    [GlobalSetup]
    public void Setup()
    {
        _fastIdTypeIds = new TypeId[Iterations];
        _tcKsTypeIds = new TcKs.TypeId.TypeId[Iterations];
        _evgregTypeIds = new global::TypeId.TypeId[Iterations];
        for (var i = 0; i < Iterations; i++)
        {
            var typeId = TypeId.New(Prefix);
            var typeIdString = typeId.ToString();
            _fastIdTypeIds[i] = typeId;
            _tcKsTypeIds[i] = TcKs.TypeId.TypeId.Parse(typeIdString);
            _evgregTypeIds[i] = global::TypeId.TypeId.Parse(typeIdString);
        }
    }
    
    [Benchmark(Baseline = true)]
    public string FastIdsBenchmark()
    {
        var result = "";
        for (var i = 0; i < Iterations; i++)
        {
            result = _fastIdTypeIds[i].ToString();
        }

        return result;
    }

    [Benchmark]
    public string TcKsBenchmark()
    {
        var result = "";
        for (var i = 0; i < Iterations; i++)
        {
            result = _tcKsTypeIds[i].ToString();
        }
        
        return result;
    }

    [Benchmark]
    public string EvgregBenchmark()
    {
        var result = "";

        for (var i = 0; i < Iterations; i++)
        {
            result = _evgregTypeIds[i].ToString();
        }

        return result;
    }
}