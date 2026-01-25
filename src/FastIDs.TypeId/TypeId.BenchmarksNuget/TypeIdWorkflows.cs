using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace TypeId.BenchmarksNuget;

[MemoryDiagnoser]
[MarkdownExporter]
[MarkdownExporterAttribute.Default]
public class TypeIdWorkflows
{
    [Params(0, 15, 63)] 
    public int TypeLength;
    
    private string _typeIdString = "";
    private FastIDs.TypeId.TypeIdDecoded _typeIdDecoded;
    private FastIDs.TypeId.TypeId _typeId;
    private string _prefix = "";
    
    private readonly string _prefixFull;
    private readonly Guid _uuidV7;

    public TypeIdWorkflows()
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
    public string Generate_ToString()
    {
        var typeIdDecoded = FastIDs.TypeId.TypeId.New(_prefix);
        return typeIdDecoded.ToString();
    }

    [Benchmark]
    public string Parse_ToString()
    {
        var typeId = FastIDs.TypeId.TypeId.Parse(_typeIdString);
        return typeId.ToString();
    }
}