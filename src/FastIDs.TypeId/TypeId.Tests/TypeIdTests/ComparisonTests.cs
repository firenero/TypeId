using System;
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

    [TestCase("bbb", "aaa", 1)]
    [TestCase("aaa", "bbb", -1)]
    public void TypeIdDecodedLexComparer_DifferentPrefix_TypesComparedLexicographically(string firstPrefix, string secondPrefix, int expected)
    {
        var first = TypeIdDecoded.New(firstPrefix);
        var second = TypeIdDecoded.New(secondPrefix);

        var result = TypeIdDecoded.Comparers.Lex.Compare(first, second);
        result.Should().Be(expected);
    }
    
    [Test]
    public void TypeIdDecodedLexComparer_SamePrefix_OlderIdIsLessThanNewer()
    {
        var first = TypeIdDecoded.New("aaa");
        var second = TypeIdDecoded.New("aaa");

        var result = TypeIdDecoded.Comparers.Lex.Compare(first, second);
        result.Should().BeLessThan(0);
    }
    
    [Test]
    public void TypeIdDecodedTimestampComparer_DifferentTimestamps_OlderIdIsLessThanNewer()
    {
        var first = TypeIdDecoded.New("aaa");
        var second = TypeIdDecoded.New("aaa");

        var result = TypeIdDecoded.Comparers.Timestamp.Compare(first, second);
        result.Should().BeLessThan(0);
    }
    
    [TestCase("aaa", "bbb", -1)]
    [TestCase("bbb", "aaa", 1)]
    [TestCase("aaa", "aaa", 0)]
    public void TypeIdDecodedTimestampComparer_SameUuidv7_TypeIsComparedLexicographically(string firstPrefix, string secondPrefix, int expected)
    {
        var first = TypeIdDecoded.FromUuidV7(firstPrefix, Guid.Parse("01953e9c-1f53-771b-a158-cb2c3c59bed4"));
        var second = TypeIdDecoded.FromUuidV7(secondPrefix, Guid.Parse("01953e9c-1f53-771b-a158-cb2c3c59bed4"));

        var result = TypeIdDecoded.Comparers.Timestamp.Compare(first, second);
        result.Should().Be(expected);
    }
    
}