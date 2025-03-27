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
    public void TypeId_Plain_Null_Serialized()
    {
        TypeId? typeId = null;
        var json = JsonSerializer.Serialize(typeId, _options);

        json.Should().Be("null");
    }

    [Test]
    public void TypeId_NestedProperty_Serialized()
    {
        var obj = new TypeIdContainer(TypeId.Parse(TypeIdStr), 42);
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}");
    }

    [Test]
    public void TypeId_NestedProperty_Null_Serialized()
    {
        var obj = new TypeIdContainer(null, 42);
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Id\":null,\"Value\":42}}");
    }

    [Test]
    public void TypeId_Collection_Serialized()
    {
        var obj = new TypeIdArrayContainer(new TypeId?[]
            { TypeId.Parse(TypeIdStr), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs") });
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}");
    }

    [Test]
    public void TypeId_Collection_WithNull_Serialized()
    {
        var obj = new TypeIdArrayContainer(new TypeId?[]
            { TypeId.Parse(TypeIdStr), null });
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Items\":[\"{TypeIdStr}\",null]}}");
    }

    [Test]
    public void TypeId_Plain_Deserialized()
    {
        var typeId = JsonSerializer.Deserialize<TypeId>($"\"{TypeIdStr}\"", _options);

        typeId.Should().Be(TypeId.Parse(TypeIdStr));
    }

    [Test]
    public void TypeId_Plain_Null_Deserialized()
    {
        var typeId = JsonSerializer.Deserialize<TypeId?>("null", _options);

        typeId.Should().BeNull();
    }

    [Test]
    public void TypeId_NestedProperty_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdContainer>($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}", _options);

        obj.Should().Be(new TypeIdContainer(TypeId.Parse(TypeIdStr), 42));
    }

    [Test]
    public void TypeId_NestedProperty_Null_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdContainer>($"{{\"Id\":null,\"Value\":42}}", _options);

        obj.Should().Be(new TypeIdContainer(null, 42));
    }

    [Test]
    public void TypeId_Collection_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdArrayContainer>(
            $"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}", _options);

        obj.Should().BeEquivalentTo(new TypeIdArrayContainer(new TypeId?[]
            { TypeId.Parse(TypeIdStr), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs") }));
    }

    [Test]
    public void TypeId_Collection_WithNull_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdArrayContainer>(
            $"{{\"Items\":[\"{TypeIdStr}\",null]}}", _options);

        obj.Should().BeEquivalentTo(new TypeIdArrayContainer(new TypeId?[]
            { TypeId.Parse(TypeIdStr), null }));
    }

    [Test]
    public void TypeId_DictionaryKey_Serialized()
    {
        var obj = new Dictionary<TypeId, string> { { TypeId.Parse(TypeIdStr), "Test" } };

        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"{TypeIdStr}\":\"Test\"}}");
    }
    
    [Test]
    public void TypeId_DictionaryValue_Null_Serialized()
    {
        var obj = new Dictionary<string, TypeId?> { { "Key", null } };

        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be("{\"Key\":null}");
    }

    [Test]
    public void TypeId_DictionaryKey_DeSerialized()
    {
        var obj = JsonSerializer.Deserialize<Dictionary<TypeId, string>>($"{{\"{TypeIdStr}\":\"Test\"}}", _options);

        obj.Should().BeEquivalentTo(new Dictionary<TypeId, string> { { TypeId.Parse(TypeIdStr), "Test" } });
    }

    [Test]
    public void TypeId_DictionaryKey_Null_DeSerialized()
    {
        Action act = () => JsonSerializer.Deserialize<Dictionary<TypeId, string>>($"{{null:\"Test\"}}", _options);

        act.Should().Throw<JsonException>();
    }

    [Test]
    public void TypeId_DictionaryValue_Null_DeSerialized()
    {
        var obj = JsonSerializer.Deserialize<Dictionary<string, TypeId?>>("{\"Key\":null}", _options);

        obj.Should().BeEquivalentTo(new Dictionary<string, TypeId?> { { "Key", null } });
    }

    private record TypeIdContainer(TypeId? Id, int Value);

    private record TypeIdArrayContainer(TypeId?[] Items);
}