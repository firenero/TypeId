using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
public class TypeIdString
{
    [Params(0, 5, 15, 63)]
    public int PrefixLength;

    private readonly string _prefixFull;
    private readonly Guid _uuidV7;
    
    private TypeId _fastIdTypeId;
    private TypeIdDecoded _fastIdTypeIdDecoded;
    private TcKs.TypeId.TypeId _tcKsTypeId;
    private global::TypeId.TypeId _cbuctokTypeId;
#if NET10_0_OR_GREATER
    private TypeSafeId.TypeId<Entity> _typeSafeIdTypeId;
#endif
    
    public TypeIdString()
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

        _fastIdTypeIdDecoded = TypeId.FromUuidV7(prefix, _uuidV7);
        _fastIdTypeId = _fastIdTypeIdDecoded.Encode();
        _tcKsTypeId = new TcKs.TypeId.TypeId(prefix, _uuidV7);
        _cbuctokTypeId = new global::TypeId.TypeId(prefix, _uuidV7);
#if NET10_0_OR_GREATER
        _typeSafeIdTypeId = new TypeSafeId.TypeId<Entity>(_uuidV7);

#pragma warning disable CS0618 // Type or member is obsolete
        global::TypeSafeId.TypeId<Entity>.SetPrefix(prefix);
#pragma warning restore CS0618 // Type or member is obsolete
#endif
    }

    [Benchmark(Baseline = true)]
    public string FastIdsDecoded()
    {
        return _fastIdTypeIdDecoded.ToString();
    }
    
    [Benchmark]
    public string FastIdsEncoded()
    {
        return _fastIdTypeId.ToString();
    }

    [Benchmark]
    public string TcKs()
    {
        return _tcKsTypeId.ToString();
    }

    [Benchmark]
    public string Cbuctok()
    {
        return _cbuctokTypeId.ToString();
    }

#if NET10_0_OR_GREATER
    [Benchmark]
    public string TypeSafeId()
    {
        return _typeSafeIdTypeId.ToString();
    }

    public record Entity;
#endif
}