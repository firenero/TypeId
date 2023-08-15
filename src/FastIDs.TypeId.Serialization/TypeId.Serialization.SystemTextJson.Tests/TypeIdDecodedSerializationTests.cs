using System.Text.Json;
using FluentAssertions;

namespace FastIDs.TypeId.Serialization.SystemTextJson.Tests;

[TestFixture]
public class TypeIdDecodedSerializationTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForTypeId();
    private const string TypeIdStr = "type_01h455vb4pex5vsknk084sn02q";

    [Test]
    public void TypeId_Plain_Serialized()
    {
        var json = JsonSerializer.Serialize(TypeId.Parse(TypeIdStr).Decode(), _options);

        json.Should().Be($"\"{TypeIdStr}\"");
    }
    
    [Test]
    public void TypeId_NestedProperty_Serialized()
    {
        var obj = new TypeIdDecodedContainer(TypeId.Parse(TypeIdStr).Decode(), 42);
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}");
    }

    [Test]
    public void TypeId_Collection_Serialized()
    {
        var obj = new TypeIdDecodedArrayContainer(new[] { TypeId.Parse(TypeIdStr).Decode(), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs").Decode() });
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}");
    }

    [Test]
    public void TypeId_Plain_Deserialized()
    {
        var typeId = JsonSerializer.Deserialize<TypeIdDecoded>($"\"{TypeIdStr}\"", _options);

        typeId.Should().Be(TypeId.Parse(TypeIdStr).Decode());
    }
    
    [Test]
    public void TypeId_NestedProperty_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdDecodedContainer>($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}", _options);

        obj.Should().Be(new TypeIdDecodedContainer(TypeId.Parse(TypeIdStr).Decode(), 42));
    }

    [Test]
    public void TypeId_Collection_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdDecodedArrayContainer>($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}", _options);

        obj.Should().BeEquivalentTo(new TypeIdDecodedArrayContainer(new[] { TypeId.Parse(TypeIdStr).Decode(), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs").Decode() }));
    }

    private record TypeIdDecodedContainer(TypeIdDecoded Id, int Value);

    private record TypeIdDecodedArrayContainer(TypeIdDecoded[] Items);
}