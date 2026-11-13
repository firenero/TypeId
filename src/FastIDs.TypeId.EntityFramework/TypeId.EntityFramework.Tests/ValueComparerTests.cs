using FluentAssertions;

namespace FastIDs.TypeId.EntityFramework.Tests;

[TestFixture]
public class ValueComparerTests
{
    [Test]
    public void TypeId_DefaultValue_CanBeHashedAndCompared()
    {
        var comparer = new TypeIdValueComparer();

        comparer.GetHashCode(default(TypeId)).Should().Be(0);
        comparer.Equals(default(TypeId), default(TypeId)).Should().BeTrue();
        comparer.Equals(default(TypeId), TypeId.New("user").Encode()).Should().BeFalse();
    }

    [Test]
    public void TypeId_EqualValues_HaveEqualHashCodes()
    {
        var comparer = new TypeIdValueComparer();
        var id = TypeId.New("user").Encode();
        var same = TypeId.Parse(id.ToString());

        comparer.Equals(id, same).Should().BeTrue();
        comparer.GetHashCode(id).Should().Be(comparer.GetHashCode(same));
        comparer.Snapshot(id).Should().Be(id);
    }

    [Test]
    public void TypeIdDecoded_DefaultValue_CanBeHashedAndCompared()
    {
        var comparer = new TypeIdDecodedValueComparer();

        var act = () => comparer.GetHashCode(default(TypeIdDecoded));

        act.Should().NotThrow();
        comparer.Equals(default(TypeIdDecoded), default(TypeIdDecoded)).Should().BeTrue();
        comparer.Equals(default(TypeIdDecoded), TypeId.New("acc")).Should().BeFalse();
    }

    [Test]
    public void TypeIdDecoded_EqualValues_HaveEqualHashCodes()
    {
        var comparer = new TypeIdDecodedValueComparer();
        var id = TypeId.New("acc");
        var same = TypeId.FromUuidV7("acc", id.Id);

        comparer.Equals(id, same).Should().BeTrue();
        comparer.GetHashCode(id).Should().Be(comparer.GetHashCode(same));
    }
}
