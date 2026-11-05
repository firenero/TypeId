using FluentAssertions;

namespace FastIDs.TypeId.EntityFrameworkCore.Tests;

[TestFixture]
public class ConverterTests
{
    private const string TypeIdStr = "type_01h455vb4pex5vsknk084sn02q";
    private static readonly Guid Uuid = new("01890a5d-ac96-774b-bcce-b302099a8057");
    private const string UuidTypeIdStr = "type_01h455vb4pex5vsknk084sn02q";

    [Test]
    public void TypeIdToString_RoundTrip()
    {
        var converter = new TypeIdToStringConverter();
        var typeId = TypeId.Parse(TypeIdStr);

        var provider = converter.ConvertToProvider(typeId);
        var model = converter.ConvertFromProvider(provider);

        provider.Should().Be(TypeIdStr);
        model.Should().Be(typeId);
    }

    [Test]
    public void TypeIdToString_WithoutType_RoundTrip()
    {
        var converter = new TypeIdToStringConverter();
        var typeId = TypeId.FromUuidV7("", Uuid).Encode();

        var provider = converter.ConvertToProvider(typeId);

        provider.Should().Be("01h455vb4pex5vsknk084sn02q");
        converter.ConvertFromProvider(provider).Should().Be(typeId);
    }

    [Test]
    public void TypeIdToString_Typed_DoesNotAllocateWhenWriting()
    {
        var typeId = TypeId.Parse(TypeIdStr);
        var toProvider = new TypeIdToStringConverter().ConvertToProviderTyped;

        toProvider(typeId).Should().BeSameAs(typeId.ToString());
    }

    [Test]
    public void TypeIdToString_InvalidProviderValue_Throws()
    {
        var converter = new TypeIdToStringConverter();

        var act = () => converter.ConvertFromProvider("Not_a_valid_id");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void TypeIdToString_MappingHints_AreAsciiMaxLength90()
    {
        var hints = new TypeIdToStringConverter().MappingHints;

        hints.Should().NotBeNull();
        hints!.Size.Should().Be(90);
        hints.IsUnicode.Should().BeFalse();
    }

    [Test]
    public void TypeIdDecodedToString_RoundTrip()
    {
        var converter = new TypeIdDecodedToStringConverter();
        var decoded = TypeId.FromUuidV7("type", Uuid);

        var provider = converter.ConvertToProvider(decoded);
        var model = converter.ConvertFromProvider(provider);

        provider.Should().Be(TypeIdStr);
        model.Should().Be(decoded);
    }

    [Test]
    public void TypeIdDecodedToString_InvalidProviderValue_Throws()
    {
        var converter = new TypeIdDecodedToStringConverter();

        var act = () => converter.ConvertFromProvider("type_zzzzzzzzzzzzzzzzzzzzzzzzzz");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void TypeIdToGuid_RoundTrip()
    {
        var converter = new TypeIdToGuidConverter("type");
        var typeId = TypeId.Parse(UuidTypeIdStr);

        var provider = converter.ConvertToProvider(typeId);
        var model = converter.ConvertFromProvider(provider);

        provider.Should().Be(Uuid);
        model.Should().Be(typeId);
        converter.Type.Should().Be("type");
    }

    [Test]
    public void TypeIdToGuid_EmptyType_RoundTrip()
    {
        var converter = new TypeIdToGuidConverter("");
        var typeId = TypeId.FromUuidV7("", Uuid).Encode();

        converter.ConvertToProvider(typeId).Should().Be(Uuid);
        converter.ConvertFromProvider(Uuid).Should().Be(typeId);
    }

    [Test]
    public void TypeIdToGuid_WrongType_Throws()
    {
        var converter = new TypeIdToGuidConverter("user");
        var typeId = TypeId.Parse(TypeIdStr);

        var act = () => converter.ConvertToProvider(typeId);

        act.Should().Throw<InvalidOperationException>().WithMessage("*'type_01h455vb4pex5vsknk084sn02q'*'user'*");
    }

    [Test]
    public void TypeIdToGuid_TypeWithSameSuffixButLongerPrefix_Throws()
    {
        // "user" is a suffix of "superuser" and "users" starts with "user": neither is the "user" type.
        var converter = new TypeIdToGuidConverter("user");

        var act1 = () => converter.ConvertToProvider(TypeId.FromUuidV7("superuser", Uuid).Encode());
        var act2 = () => converter.ConvertToProvider(TypeId.FromUuidV7("users", Uuid).Encode());
        var act3 = () => converter.ConvertToProvider(TypeId.FromUuidV7("", Uuid).Encode());

        act1.Should().Throw<InvalidOperationException>();
        act2.Should().Throw<InvalidOperationException>();
        act3.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TypeIdToGuid_DefaultValue_Throws()
    {
        var converter = new TypeIdToGuidConverter("user");

        var act = () => converter.ConvertToProvider(default(TypeId));

        act.Should().Throw<InvalidOperationException>().WithMessage("*default*");
    }

    [Test]
    public void TypeIdToGuid_InvalidType_ThrowsInConstructor()
    {
        var act = () => new TypeIdToGuidConverter("Not-Valid");

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void TypeIdToGuid_NullType_ThrowsInConstructor()
    {
        var act = () => new TypeIdToGuidConverter(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void TypeIdDecodedToGuid_RoundTrip()
    {
        var converter = new TypeIdDecodedToGuidConverter("type");
        var decoded = TypeId.FromUuidV7("type", Uuid);

        var provider = converter.ConvertToProvider(decoded);
        var model = converter.ConvertFromProvider(provider);

        provider.Should().Be(Uuid);
        model.Should().Be(decoded);
    }

    [Test]
    public void TypeIdDecodedToGuid_WrongType_Throws()
    {
        var converter = new TypeIdDecodedToGuidConverter("user");

        var act = () => converter.ConvertToProvider(TypeId.FromUuidV7("type", Uuid));

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TypeIdDecodedToGuid_DefaultValue_Throws()
    {
        var converter = new TypeIdDecodedToGuidConverter("user");

        var act = () => converter.ConvertToProvider(default(TypeIdDecoded));

        act.Should().Throw<InvalidOperationException>().WithMessage("*default*");
    }

    [Test]
    public void TypeIdDecodedToGuid_InvalidType_ThrowsInConstructor()
    {
        var act = () => new TypeIdDecodedToGuidConverter("Not-Valid");

        act.Should().Throw<FormatException>();
    }
}
