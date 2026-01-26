using System.Text;
using BenchmarkDotNet.Attributes;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
[MarkdownExporterAttribute.GitHub]
public class TypeIdParsing
{
    [Params(0, 5, 15, 63)] 
    public int PrefixLength;

    private string _typeIdString = "";
    
    private readonly string _prefixFull;
    private readonly Guid _uuidV7;

    public TypeIdParsing()
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
#pragma warning disable CS0618 // Type or member is obsolete
        TypeSafeId.TypeId<Entity>.SetPrefix(prefix);
#pragma warning restore CS0618 // Type or member is obsolete
#endif
    }

    [Benchmark(Baseline = true)]
    public TypeId FastIdsParse()
    {
        return TypeId.Parse(_typeIdString);
    }
    
    [Benchmark]
    public TypeId FastIdsTryParse()
    {
        TypeId.TryParse(_typeIdString, out var typeId);
        return typeId;
    }

    [Benchmark]
    public TcKs.TypeId.TypeId TcKsParse()
    {
        return TcKs.TypeId.TypeId.Parse(_typeIdString);
    }
    
    [Benchmark]
    public TcKs.TypeId.TypeId TcKsTryParse()
    {
        TcKs.TypeId.TypeId.TryParse(_typeIdString, out var typeId);
        return typeId;
    }

    [Benchmark]
    public global::TypeId.TypeId CbuctokParse()
    {
        return global::TypeId.TypeId.Parse(_typeIdString);
    }
    
    [Benchmark]
    public global::TypeId.TypeId CbuctokTryParse()
    {
        global::TypeId.TypeId.TryParse(_typeIdString, out var typeId);
        return typeId;
    }

#if NET10_0_OR_GREATER
    [Benchmark]
    public TypeSafeId.TypeId<Entity> TypeSafeIdParse()
    {
        return TypeSafeId.TypeId<Entity>.Parse(_typeIdString);
    }

    [Benchmark]
    public TypeSafeId.TypeId<Entity> TypeSafeIdTryParse()
    {
        TypeSafeId.TypeId<Entity>.TryParse(_typeIdString, out var typeId);
        return typeId;
    }

    public record Entity;
#endif
}