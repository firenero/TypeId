using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.GitHub]
[MarkdownExporterAttribute.Default]
public class TypeIdString
{
    [Params(1_000_000)]
    public int Iterations;
    
    [Params(5, 10, 63)]
    public int PrefixLength;

    private TypeId[] _fastIdTypeIds;
    private TcKs.TypeId.TypeId[] _tcKsTypeIds;
    private global::TypeId.TypeId[] _cbuctokTypeIds;

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
        
        _fastIdTypeIds = new TypeId[Iterations];
        _tcKsTypeIds = new TcKs.TypeId.TypeId[Iterations];
        _cbuctokTypeIds = new global::TypeId.TypeId[Iterations];
        for (var i = 0; i < Iterations; i++)
        {
            var typeId = TypeId.New(prefix);
            var typeIdString = typeId.ToString();
            _fastIdTypeIds[i] = typeId;
            _tcKsTypeIds[i] = TcKs.TypeId.TypeId.Parse(typeIdString);
            _cbuctokTypeIds[i] = global::TypeId.TypeId.Parse(typeIdString);
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
    public string CbuctokBenchmark()
    {
        var result = "";

        for (var i = 0; i < Iterations; i++)
        {
            result = _cbuctokTypeIds[i].ToString();
        }

        return result;
    }
}