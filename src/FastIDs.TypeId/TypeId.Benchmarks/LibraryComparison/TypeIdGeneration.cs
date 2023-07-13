using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.GitHub]
[MarkdownExporterAttribute.Default]
public class TypeIdGeneration
{
    [Params(1_000_000)]
    public int Iterations;

    [Params(5, 10, 63)]
    public int PrefixLength;
    
    private string Prefix;
    
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
        Prefix = PrefixLength > 0 ? sb.ToString() : "";
    }
    
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
    public TypeId FastIdsWithoutValidationBenchmark()
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
    public global::TypeId.TypeId CbuctokBenchmark()
    {
        global::TypeId.TypeId typeId = default;

        for (var i = 0; i < Iterations; i++)
        {
            typeId = global::TypeId.TypeId.NewTypeId(Prefix);
        }

        return typeId;
    }
}