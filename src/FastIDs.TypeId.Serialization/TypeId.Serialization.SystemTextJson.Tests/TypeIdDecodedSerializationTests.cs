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
    public void TypeId_NestedProperty_Null_Serialized()
    {
        var obj = new TypeIdDecodedContainer(null, 42);
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be("{\"Id\":null,\"Value\":42}");
    }

    [Test]
    public void TypeId_Collection_Serialized()
    {
        var obj = new TypeIdDecodedArrayContainer(new TypeIdDecoded?[]
            { TypeId.Parse(TypeIdStr).Decode(), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs").Decode() });
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}");
    }

    [Test]
    public void TypeId_Collection_WithNull_Serialized()
    {
        var obj = new TypeIdDecodedArrayContainer(new TypeIdDecoded?[] { TypeId.Parse(TypeIdStr).Decode(), null });
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"Items\":[\"{TypeIdStr}\",null]}}");
    }

    [Test]
    public void TypeId_Plain_Deserialized()
    {
        var typeId = JsonSerializer.Deserialize<TypeIdDecoded>($"\"{TypeIdStr}\"", _options);

        typeId.Should().Be(TypeId.Parse(TypeIdStr).Decode());
    }

    [Test]
    public void TypeId_Plain_Null_Deserialized()
    {
        var typeId = JsonSerializer.Deserialize<TypeIdDecoded?>("null", _options);

        typeId.Should().BeNull();
    }

    [Test]
    public void TypeId_NestedProperty_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdDecodedContainer>($"{{\"Id\":\"{TypeIdStr}\",\"Value\":42}}",
            _options);

        obj.Should().Be(new TypeIdDecodedContainer(TypeId.Parse(TypeIdStr).Decode(), 42));
    }

    [Test]
    public void TypeId_NestedProperty_Null_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdDecodedContainer>("{\"Id\":null,\"Value\":42}", _options);

        obj.Should().Be(new TypeIdDecodedContainer(null, 42));
    }

    [Test]
    public void TypeId_Collection_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdDecodedArrayContainer>(
            $"{{\"Items\":[\"{TypeIdStr}\",\"prefix_0123456789abcdefghjkmnpqrs\"]}}", _options);

        obj.Should().BeEquivalentTo(new TypeIdDecodedArrayContainer(new TypeIdDecoded?[]
            { TypeId.Parse(TypeIdStr).Decode(), TypeId.Parse("prefix_0123456789abcdefghjkmnpqrs").Decode() }));
    }

    [Test]
    public void TypeId_Collection_WithNull_Deserialized()
    {
        var obj = JsonSerializer.Deserialize<TypeIdDecodedArrayContainer>($"{{\"Items\":[\"{TypeIdStr}\",null]}}",
            _options);

        obj.Should().BeEquivalentTo(new TypeIdDecodedArrayContainer(new TypeIdDecoded?[]
            { TypeId.Parse(TypeIdStr).Decode(), null }));
    }

    [Test]
    public void TypeId_DictionaryKey_Serialized()
    {
        var obj = new Dictionary<TypeIdDecoded, string> { { TypeId.Parse(TypeIdStr).Decode(), "Test" } };

        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be($"{{\"{TypeIdStr}\":\"Test\"}}");
    }

    [Test]
    public void TypeId_DictionaryValue_Null_Serialized()
    {
        var obj = new Dictionary<string, TypeIdDecoded?> { { "Key", null } };

        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be("{\"Key\":null}");
    }

    [Test]
    public void TypeId_DictionaryKey_DeSerialized()
    {
        var obj = JsonSerializer.Deserialize<Dictionary<TypeIdDecoded, string>>($"{{\"{TypeIdStr}\":\"Test\"}}",
            _options);

        obj.Should().BeEquivalentTo(new Dictionary<TypeIdDecoded, string>
            { { TypeId.Parse(TypeIdStr).Decode(), "Test" } });
    }

    [Test]
    public void TypeId_DictionaryKey_Null_DeSerialized()
    {
        // Deserializing a dictionary with a null key is invalid in JSON; test for exception
        Action act = () => JsonSerializer.Deserialize<Dictionary<TypeIdDecoded, string>>("{null:\"Test\"}", _options);

        act.Should().Throw<JsonException>();
    }

    [Test]
    public void TypeId_DictionaryValue_Null_DeSerialized()
    {
        var obj = JsonSerializer.Deserialize<Dictionary<string, TypeIdDecoded?>>("{\"Key\":null}", _options);

        obj.Should().BeEquivalentTo(new Dictionary<string, TypeIdDecoded?> { { "Key", null } });
    }

    private record TypeIdDecodedContainer(TypeIdDecoded? Id, int Value);

    private record TypeIdDecodedArrayContainer(TypeIdDecoded?[] Items);
}