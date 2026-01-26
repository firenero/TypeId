using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace TypeId.BenchmarksNuget;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
public class TypeIdBasicOps
{
    [Params(0, 15, 63)] 
    public int TypeLength;
    
    private string _typeIdString = "";
    private FastIDs.TypeId.TypeIdDecoded _typeIdDecoded;
    private FastIDs.TypeId.TypeId _typeId;
    private string _prefix = "";
    
    private readonly string _prefixFull;
    private readonly Guid _uuidV7;

    public TypeIdBasicOps()
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
        _prefix = _prefixFull[..TypeLength];
        _typeIdDecoded = FastIDs.TypeId.TypeId.FromUuidV7(_prefix, _uuidV7);
        _typeId = _typeIdDecoded.Encode();
        _typeIdString = _typeId.ToString();
    }
    
    [Benchmark]
    public FastIDs.TypeId.TypeId Parse()
    {
        return FastIDs.TypeId.TypeId.Parse(_typeIdString);
    }
    
    [Benchmark]
    public FastIDs.TypeId.TypeId TryParse()
    {
        FastIDs.TypeId.TypeId.TryParse(_typeIdString, out var typeId);
        return typeId;
    }

    [Benchmark]
    public FastIDs.TypeId.TypeIdDecoded Decode()
    {
        return _typeId.Decode();
    }

    [Benchmark]
    public FastIDs.TypeId.TypeId Encode()
    {
        return _typeIdDecoded.Encode();
    }

    [Benchmark]
    public FastIDs.TypeId.TypeIdDecoded Generate()
    {
        return FastIDs.TypeId.TypeId.New(_prefix);
    }
    
    [Benchmark]
    public FastIDs.TypeId.TypeIdDecoded GenerateUnsafe()
    {
        return FastIDs.TypeId.TypeId.New(_prefix, false);
    }

    [Benchmark]
    public string DecodedToString()
    {
        return _typeIdDecoded.ToString();
    }

    [Benchmark]
    public string EncodedToString()
    {
        return _typeId.ToString();
    }

    [Benchmark]
    public DateTimeOffset TimestampFromDecoded()
    {
        return _typeIdDecoded.GetTimestamp();
    }

    [Benchmark]
    public string SuffixFromDecoded()
    {
        return _typeIdDecoded.GetSuffix();
    }
    
    [Benchmark]
    public char SuffixSpanFromDecoded()
    {
        const int suffixLength = 26;
        Span<char> buffer = stackalloc char[suffixLength];
        var charsWritten = _typeIdDecoded.GetSuffix(buffer);
        return buffer[charsWritten - 1];
    }
    
    [Benchmark]
    public ReadOnlySpan<char> SuffixSpanFromEncoded()
    {
        return _typeId.Suffix;
    }

    [Benchmark]
    public string TypeFromDecoded()
    {
        return _typeIdDecoded.Type;
    }
    
    [Benchmark]
    public ReadOnlySpan<char> TypeFromEncoded()
    {
        return _typeId.Type;
    }
}