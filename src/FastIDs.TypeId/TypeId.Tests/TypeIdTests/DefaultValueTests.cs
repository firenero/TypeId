using System.Collections.Generic;
using FastIDs.TypeId;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class DefaultValueTests
{
    [Test]
    public void TypeId_Default_GetHashCode_DoesNotThrow()
    {
        var act = () => default(TypeId).GetHashCode();

        act.Should().NotThrow();
    }

    [Test]
    public void TypeId_Default_CanBeUsedAsDictionaryKey()
    {
        var id = TypeId.New("user").Encode();
        var dictionary = new Dictionary<TypeId, int> { [default] = 1, [id] = 2 };

        dictionary[default].Should().Be(1);
        dictionary[id].Should().Be(2);
    }

    [Test]
    public void TypeId_Default_CanBeAddedToHashSet()
    {
        var set = new HashSet<TypeId> { default, default };

        set.Should().HaveCount(1);
    }

    [Test]
    public void TypeId_EqualValues_HaveEqualHashCodes()
    {
        var id = TypeId.New("user").Encode();
        var same = TypeId.Parse(id.ToString());

        same.GetHashCode().Should().Be(id.GetHashCode());
    }
}
