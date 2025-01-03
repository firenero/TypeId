using FastIDs.TypeId;
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
}