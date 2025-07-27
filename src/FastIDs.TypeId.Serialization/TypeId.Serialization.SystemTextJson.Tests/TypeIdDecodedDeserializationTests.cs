using System.Text.Json;
using FluentAssertions;

namespace FastIDs.TypeId.Serialization.SystemTextJson.Tests;

[TestFixture]
public class TypeIdDecodedDeserializationTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForTypeId();
    private const string InvalidTypeIdStr = "type_01h455vb4pex5vsknk084sn02L";  // 'L' is not valid base32
    
    [Test]
    public void TypeId_ParsingErrorDuringRead_ShouldBeConvertedToJsonException()
    {
        // arrange
        const string json = $$"""
            {
                "Id": "{{InvalidTypeIdStr}}",
                "Value": 123 
            }
            """;

        // act
        var act = () => JsonSerializer.Deserialize<TypeIdDecodedContainer>(json, _options);

        // assert
        act.Should().Throw<JsonException>()
            .Where(x => x.LineNumber == 1)
            .Where(x => x.BytePositionInLine != null)
            .Where(x => x.Path == "$.Id")
            .WithInnerException<FormatException>();
    }
    
    [Test]
    public void TypeId_ParsingErrorDuringReadAsPropertyName_ShouldBeConvertedToJsonException()
    {
        // arrange
        const string json = $$"""
            {
                "Id": "{{InvalidTypeIdStr}}",
                "Value": 123 
            }
            """;

        // act
        var act = () => JsonSerializer.Deserialize<Dictionary<TypeIdDecoded, int>>(json, _options);

        // assert
        act.Should().Throw<JsonException>()
            .Where(x => x.LineNumber == 1)
            .Where(x => x.BytePositionInLine != null)
            .Where(x => x.Path == "$.Id")
            .WithInnerException<FormatException>();
    }
    
    private record TypeIdDecodedContainer(TypeIdDecoded Id, int Value);
}
