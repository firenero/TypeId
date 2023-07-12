using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
public class TypeIdString
{
    [Params(10, 1000, 1_000_000, 1_000_000_000)]
    public int Iterations;
    
    private const string Prefix = "prefix";
    
    [Benchmark(Baseline = true)]
    public string FastIdsBenchmark()
    {
        var result = "";
        for (var i = 0; i < Iterations; i++)
        {
            var typeId = TypeId.New(Prefix);
            result = typeId.ToString();
        }

        return result;
    }
    
    [Benchmark]
    public string FastIdsUnsafeBenchmark()
    {
        var result = "";
        for (var i = 0; i < Iterations; i++)
        {
            var typeId = TypeId.New(Prefix, false);
            result = typeId.ToString();
        }

        return result;
    }

    [Benchmark]
    public string TcKsBenchmark()
    {
        var result = "";
        for (var i = 0; i < Iterations; i++)
        {
            var typeId = TcKs.TypeId.TypeId.NewId(Prefix);
            result = typeId.ToString();
        }
        
        return result;
    }

    [Benchmark]
    public string EvgregBenchmark()
    {
        var result = "";

        for (var i = 0; i < Iterations; i++)
        {
            var typeId = global::TypeId.TypeId.NewTypeId(Prefix);
            result = typeId.ToString();
        }

        return result;
    }
}