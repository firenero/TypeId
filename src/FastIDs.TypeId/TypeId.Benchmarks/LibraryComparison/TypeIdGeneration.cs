using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
public class TypeIdGeneration
{
    [Params(0, 5, 10, 30, 63)]
    public int PrefixLength;
    
    private string _prefix = "";
    private readonly string _prefixFull;

    public TypeIdGeneration()
    {
        var random = new Random(42);
        var sb = new StringBuilder(63);
        for (var i = 0; i < 63; i++)
        {
            var letter = (char) random.Next('a', 'z');
            sb.Append(letter);
        }
        _prefixFull = sb.ToString();
    }

    [GlobalSetup]
    public void Setup()
    {
        _prefix = _prefixFull[..PrefixLength];

        TypeSafeId.TypeId<Entity>.SetPrefix(_prefix);
    }

    [Benchmark(Baseline = true)]
    public TypeIdDecoded FastIdsBenchmark()
    {
        return TypeId.New(_prefix);
    }

    [Benchmark]
    public TypeIdDecoded FastIdsNoCheckBenchmark()
    {
        return TypeId.New(_prefix, false);
    }

    [Benchmark]
    public TcKs.TypeId.TypeId TcKsBenchmark()
    {
        return TcKs.TypeId.TypeId.NewId(_prefix);
    }

    [Benchmark]
    public global::TypeId.TypeId CbuctokBenchmark()
    {
        return global::TypeId.TypeId.NewTypeId(_prefix);
    }

    [Benchmark]
    public TypeSafeId.TypeId<Entity> TypeSafeIdBenchmark()
    {
        return TypeSafeId.TypeId<Entity>.Create();
    }

    public record Entity;
}