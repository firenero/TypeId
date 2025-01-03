using FastIDs.TypeId;
using FastIDs.TypeId.Uuid;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class ComparisonTests
{
    [Test]
    public void TypeId_DifferentPrefix_PrefixesComparedLexicographically()
    {
        var first = TypeId.New("bbb").Encode();
        var second = TypeId.New("aaa").Encode();

        first.Should().BeGreaterThan(second);
    }
    
    [Test]
    public void TypeId_SamePrefix_OlderIdIsLessThanNewer()
    {
        var first = TypeId.New("aaa").Encode();
        var second = TypeId.New("aaa").Encode();

        first.Should().BeLessThan(second);
    }

    [Test]
    public void UuidV7_DifferentTimestamps_OlderIdIsLessThanNewer()
    {
        var first = TypeId.New("aaa");
        var second = TypeId.New("aaa");

        var result = UuidComparer.Instance.Compare(first.Id, second.Id);
        result.Should().BeLessThan(0);
    }
    
    [Test]
    public void UuidV7_DifferentTimestamps_NewerIsGreaterThanOlder()
    {
        var first = TypeId.New("aaa");
        var second = TypeId.New("aaa");

        var result = UuidComparer.Instance.Compare(second.Id, first.Id);
        result.Should().BeGreaterThan(0);
    }
    
    [Test]
    public void UuidV7_SameTimestamps_IdsAreEqual()
    {
        var first = TypeId.New("aaa");

        var result = UuidComparer.Instance.Compare(first.Id, first.Id);
        result.Should().Be(0);
    }
}