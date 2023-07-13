using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
public class TypeIdGeneration
{
    [Params(10_000_000)]
    public int Iterations;

    private const string Prefix = "prefix";
    
    [Benchmark(Baseline = true)]
    public TypeId FastIdsBenchmark()
    {
        TypeId typeId = default;
        for (var i = 0; i < Iterations; i++)
        {
            typeId = TypeId.New(Prefix);
        }

        return typeId;
    }
    
    [Benchmark]
    public TypeId FastIdsWithoutValidationBenchmark2()
    {
        TypeId typeId = default;
        for (var i = 0; i < Iterations; i++)
        {
            typeId = TypeId.New(Prefix, false);
        }

        return typeId;
    }

    [Benchmark]
    public TcKs.TypeId.TypeId TcKsBenchmark()
    {
        TcKs.TypeId.TypeId typeId = default;
        for (var i = 0; i < Iterations; i++)
        {
            typeId = TcKs.TypeId.TypeId.NewId(Prefix);
        }
        
        return typeId;
    }

    [Benchmark]
    public global::TypeId.TypeId EvgregBenchmark()
    {
        global::TypeId.TypeId typeId = default;

        for (var i = 0; i < Iterations; i++)
        {
            typeId = global::TypeId.TypeId.NewTypeId(Prefix);
        }

        return typeId;
    }
}