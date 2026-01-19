using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
public class TypeIdRetrieveFlow
{
    [Params(0, 5, 10, 30, 63)]
    public int PrefixLength;

    private string _typeIdString = "";
    
    private readonly string _prefixFull;
    private readonly Guid _uuidV7;

    public TypeIdRetrieveFlow()
    {
        var random = new Random(42);
        var sb = new StringBuilder(63);
        for (var i = 0; i < 63; i++)
        {
            var letter = (char) random.Next('a', 'z');
            sb.Append(letter);
        }
        _prefixFull = sb.ToString();
        _uuidV7 = new Guid("01890a5d-ac96-774b-bcce-b302099a8057");
    }

    [GlobalSetup]
    public void Setup()
    {
        var prefix = _prefixFull[..PrefixLength];
        _typeIdString = TypeId.FromUuidV7(prefix, _uuidV7).ToString();

#if NET10_0_OR_GREATER
        TypeSafeId.TypeId<Entity>.SetPrefix(prefix);
#endif
    }
    
    [Benchmark(Baseline = true)]
    public string FastIds()
    {
        var typeId = TypeId.Parse(_typeIdString);
        return typeId.ToString();
    }
    
    [Benchmark]
    public string FastIdsDecode()
    {
        var typeId = TypeId.Parse(_typeIdString);
        var decoded = typeId.Decode();
        return decoded.ToString();
    }

    [Benchmark]
    public string TcKsBenchmark()
    {
        var typeId = TcKs.TypeId.TypeId.Parse(_typeIdString);
        return typeId.ToString();
    }

    [Benchmark]
    public string CbuctokBenchmark()
    {
        var typeId = global::TypeId.TypeId.Parse(_typeIdString);
        return typeId.ToString();
    }

#if NET10_0_OR_GREATER
    [Benchmark]
    public string TypeSafeIdBenchmark()
    {
        var typeId = TypeSafeId.TypeId<Entity>.Parse(_typeIdString);
        return typeId.ToString();
    }

    public record Entity;
#endif
}