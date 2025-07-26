using FluentAssertions;
using Newtonsoft.Json;

namespace FastIDs.TypeId.Serialization.JsonNet.Tests;

[TestFixture]
public class TypeIdDecodedDeserializationTests
{
    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings().ConfigureForTypeId();
    private const string InvalidTypeIdStr = "type_01h455vb4pex5vsknk084sn02L"; // 'L' is not valid base32

    [Test]
    public void TypeId_ParsingError_ShouldBeConvertedToJsonException()
    {
        // arrange
        const string json = $$"""
            {
                "Id": "{{InvalidTypeIdStr}}",
                "Value": 123 
            }
            """;

        // act
        var act = () => JsonConvert.DeserializeObject<TypeIdDecodedContainer>(json, _settings);

        // assert
        act.Should()
            .Throw<JsonSerializationException>()
            .Where(x => x.LineNumber == 2)
            .Where(x => x.LinePosition > 0)
            .WithInnerException<FormatException>();
    }

    private record TypeIdDecodedContainer(TypeIdDecoded Id, int Value);
}
