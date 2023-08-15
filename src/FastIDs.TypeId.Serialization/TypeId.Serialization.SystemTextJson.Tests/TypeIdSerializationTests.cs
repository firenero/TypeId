using System.Text.Json;
using FluentAssertions;

namespace FastIDs.TypeId.Serialization.SystemTextJson.Tests;

[TestFixture]
public class TypeIdSerializationTests
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions().ConfigureForTypeId();
    private const string TypeIdStr = "type_01h455vb4pex5vsknk084sn02q";

    [Test]
    public void TypeId_Plain_Serialized()
    {
        var json = JsonSerializer.Serialize(TypeId.Parse(TypeIdStr), _options);

        json.Should().Be($"\"{TypeIdStr}\"");
    }
    
    [Test]
    public void TypeId_NestedProperty_Serialized()
    {
        var obj = new TypeIdContainer(TypeId.Parse(TypeIdStr), 42);
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}");
    }

    [Test]
    public void TypeId_Collection_Serialized()
    {
        var obj = new TypeIdArrayContainer(new[] { TypeId.Parse(TypeIdStr), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs") });
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}");
    }

    [Test]
    public void TypeId_Plain_Deserialized()
    {
        var typeId = JsonSerializer.Deserialize<TypeId>($"\"{TypeIdStr}\"", _options);

        typeId.Should().Be(TypeId.Parse(TypeIdStr));
    }
    
    [Test]
    public void TypeId_NestedProperty_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdContainer>($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}", _options);

        obj.Should().Be(new TypeIdContainer(TypeId.Parse(TypeIdStr), 42));
    }

    [Test]
    public void TypeId_Collection_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdArrayContainer>($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}", _options);

        obj.Should().BeEquivalentTo(new TypeIdArrayContainer(new[] { TypeId.Parse(TypeIdStr), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs") }));
    }

    private record TypeIdContainer(TypeId Id, int Value);

    private record TypeIdArrayContainer(TypeId[] Items);
}