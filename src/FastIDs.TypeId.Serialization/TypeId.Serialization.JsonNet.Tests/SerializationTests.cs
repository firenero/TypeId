using FluentAssertions;
using Newtonsoft.Json;

namespace FastIDs.TypeId.Serialization.JsonNet.Tests;

[TestFixture]
public class SerializationTests
{
    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings().ConfigureForTypeId();
    private const string TypeIdStr = "type_01h455vb4pex5vsknk084sn02q";

    [Test]
    public void TypeId_Plain_Serialized()
    {
        var json = JsonConvert.SerializeObject(TypeId.Parse(TypeIdStr), _settings);

        json.Should().Be($"\"{TypeIdStr}\"");
    }
    
    [Test]
    public void TypeId_NestedProperty_Serialized()
    {
        var obj = new TypeIdContainer(TypeId.Parse(TypeIdStr), 42);
        var json = JsonConvert.SerializeObject(obj, _settings);

        json.Should().Be($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}");
    }

    [Test]
    public void TypeId_Collection_Serialized()
    {
        var obj = new TypeIdArrayContainer(new[] { TypeId.Parse(TypeIdStr), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs") });
        var json = JsonConvert.SerializeObject(obj, _settings);

        json.Should().Be($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}");
    }

    [Test]
    public void TypeId_Plain_Deserialized()
    {
        var typeId = JsonConvert.DeserializeObject<TypeId>($"\"{TypeIdStr}\"", _settings);

        typeId.Should().Be(TypeId.Parse(TypeIdStr));
    }
    
    [Test]
    public void TypeId_NestedProperty_Deserialized()
    {
        var obj = JsonConvert.DeserializeObject<TypeIdContainer>($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}", _settings);

        obj.Should().Be(new TypeIdContainer(TypeId.Parse(TypeIdStr), 42));
    }

    [Test]
    public void TypeId_Collection_Deserialized()
    {
        var obj = JsonConvert.DeserializeObject<TypeIdArrayContainer>($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}", _settings);

        obj.Should().BeEquivalentTo(new TypeIdArrayContainer(new[] { TypeId.Parse(TypeIdStr), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs") }));
    }

    private record TypeIdContainer(TypeId Id, int Value);

    private record TypeIdArrayContainer(TypeId[] Items);
}