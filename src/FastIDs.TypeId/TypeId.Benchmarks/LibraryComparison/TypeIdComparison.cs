using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace FastIDs.TypeId.Benchmarks.LibraryComparison;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class TypeIdComparison
{
    [Params(0, 5, 10, 30, 63)] 
    public int PrefixLength;

    private string _suffix = "01h455vb4pex5vsknk084sn02q";
    
    private TypeId[] _fastIdTypeIds = Array.Empty<TypeId>();
    private TypeIdDecoded[] _fastIdTypeIdsDecoded = Array.Empty<TypeIdDecoded>();
    private TcKs.TypeId.TypeId[] _tcKsTypeIds = Array.Empty<TcKs.TypeId.TypeId>();
    private global::TypeId.TypeId[] _cbuctokTypeIds = Array.Empty<global::TypeId.TypeId>();
#if NET10_0_OR_GREATER
    private TypeSafeId.TypeId<Entity>[] _typeSafeIds = Array.Empty<TypeSafeId.TypeId<Entity>>();
#endif

    private readonly string _prefixFull;

    public TypeIdComparison()
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
        var prefix = _prefixFull[..PrefixLength];
        var typeIdStr = PrefixLength > 0
            ? $"{prefix}_{_suffix}"
            : _suffix;

#if NET10_0_OR_GREATER
        TypeSafeId.TypeId<Entity>.SetPrefix(prefix);
#endif

        _fastIdTypeIds = new[] { TypeId.Parse(typeIdStr), TypeId.Parse(typeIdStr) };
        _fastIdTypeIdsDecoded = new[] { TypeId.Parse(typeIdStr).Decode(), TypeId.Parse(typeIdStr).Decode() };

        _tcKsTypeIds = Array.Empty<TcKs.TypeId.TypeId>();
        if (TcKs.TypeId.TypeId.TryParse(typeIdStr, out var parsed))
        {
            _tcKsTypeIds = new[] { parsed, parsed };
        }

        _cbuctokTypeIds = new[] { global::TypeId.TypeId.Parse(typeIdStr), global::TypeId.TypeId.Parse(typeIdStr) };
#if NET10_0_OR_GREATER
        _typeSafeIds = new[] { TypeSafeId.TypeId<Entity>.Parse(typeIdStr), TypeSafeId.TypeId<Entity>.Parse(typeIdStr) };
#endif
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Equality")]
    public bool FastIdsEquals() => _fastIdTypeIds[0] == _fastIdTypeIds[1];
    
    [Benchmark]
    [BenchmarkCategory("Equality")]
    public bool FastIdsDecodedEquals() => _fastIdTypeIdsDecoded[0] == _fastIdTypeIdsDecoded[1];

    [Benchmark]
    [BenchmarkCategory("Equality")]
    public bool TcKsEquals() => _tcKsTypeIds[0] == _tcKsTypeIds[1];

    [Benchmark]
    [BenchmarkCategory("Equality")]
    public bool CbuctokEquals() => _cbuctokTypeIds[0] == _cbuctokTypeIds[1];

#if NET10_0_OR_GREATER
    [Benchmark]
    [BenchmarkCategory("Equality")]
    public bool TypeSafeIdEquals() => _typeSafeIds[0] == _typeSafeIds[1];
#endif

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HashCode")]
    public int FastIdsHash() => _fastIdTypeIds[0].GetHashCode();
    
    [Benchmark]
    [BenchmarkCategory("HashCode")]
    public int FastIdsDecodedHash() => _fastIdTypeIdsDecoded[0].GetHashCode();

    [Benchmark]
    [BenchmarkCategory("HashCode")]
    public int TcKsHash() => _tcKsTypeIds[0].GetHashCode();

    [Benchmark]
    [BenchmarkCategory("HashCode")]
    public int CbuctokHash() => _cbuctokTypeIds[0].GetHashCode();

#if NET10_0_OR_GREATER
    [Benchmark]
    [BenchmarkCategory("HashCode")]
    public int TypeSafeIdHash() => _typeSafeIds[0].GetHashCode();
#endif

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Prefix")]
    public string FastIdsPrefixString() => _fastIdTypeIds[0].Type.ToString();

    [Benchmark]
    [BenchmarkCategory("Prefix")]
    public ReadOnlySpan<char> FastIdsPrefixSpan() => _fastIdTypeIds[0].Type;

    [Benchmark]
    [BenchmarkCategory("Prefix")]
    public string FastIdsDecodedPrefix() => _fastIdTypeIdsDecoded[0].Type;

    [Benchmark]
    [BenchmarkCategory("Prefix")]
    public string TcKsPrefix() => _tcKsTypeIds[0].Type;

    [Benchmark]
    [BenchmarkCategory("Prefix")]
    public string CbuctokPrefix() => _cbuctokTypeIds[0].Type;

#if NET10_0_OR_GREATER
    [Benchmark]
    [BenchmarkCategory("Prefix")]
    public string TypeSafeIdPrefix() => TypeSafeId.TypeId<Entity>.Prefix;
#endif

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Id")]
    public Guid FastIdsId() => _fastIdTypeIds[0].Decode().Id;
    
    [Benchmark]
    [BenchmarkCategory("Id")]
    public Guid FastIdsDecodedId() => _fastIdTypeIdsDecoded[0].Id;

    [Benchmark]
    [BenchmarkCategory("Id")]
    public Guid TcKsId() => _tcKsTypeIds[0].Id;

    [Benchmark]
    [BenchmarkCategory("Id")]
    public string CbuctokId() => _cbuctokTypeIds[0].Id;

#if NET10_0_OR_GREATER
    [Benchmark]
    [BenchmarkCategory("Id")]
    public Guid TypeSafeIdId() => _typeSafeIds[0].Uuid;
#endif

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Suffix")]
    public string FastIdsSuffixString() => _fastIdTypeIds[0].Suffix.ToString();
    
    [Benchmark]
    [BenchmarkCategory("Suffix")]
    public ReadOnlySpan<char> FastIdsSuffixSpan() => _fastIdTypeIds[0].Suffix;
    
    [Benchmark]
    [BenchmarkCategory("Suffix")]
    public string FastIdsDecodedSuffix() => _fastIdTypeIdsDecoded[0].GetSuffix();

    [Benchmark]
    [BenchmarkCategory("Suffix")]
    public string TcKsSuffix() => _tcKsTypeIds[0].Suffix;

#if NET10_0_OR_GREATER
    [Benchmark]
    [BenchmarkCategory("Suffix")]
    public string TypeSafeIdSuffix() => _typeSafeIds[0].GetSuffix();

    [Benchmark]
    [BenchmarkCategory("Suffix")]
    public int TypeSafeIdSuffixSpan()
    {
        Span<char> buffer = stackalloc char[26];
        return _typeSafeIds[0].GetSuffix(buffer);
    }

    public record Entity;
#endif
}