using System;
using FastIDs.TypeId;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class GenerationTests
{
    [TestCaseSource(typeof(TestCases), nameof(TestCases.WithPrefix))]
    public void FromUuidV7_TypeIdCreated(string typeIdStr, Guid uuidV7, string prefix)
    {
        var typeId = TypeId.FromUuidV7(prefix, uuidV7);

        typeId.ToString().Should().Be(typeIdStr);
    }

    [TestCase("prefix")]
    [TestCase("type")]
    [TestCase("")]
    public void New_WithType_TypeIdCreated(string type)
    {
        var typeId = TypeId.New(type);

        typeId.Type.Should().Be(type);
        typeId.Id.Should().NotBeEmpty();
    }

    [TestCaseSource(nameof(InvalidTypes))]
    public void New_IncorrectType_FormatExceptionThrown(string type)
    {
        var act = () => TypeId.New(type);
        act.Should().Throw<FormatException>();
    }
    
    [TestCaseSource(nameof(InvalidTypes))]
    public void FromUuidV7_IncorrectType_FormatExceptionThrown(string type)
    {
        var act = () => TypeId.FromUuidV7(type, Guid.NewGuid());
        act.Should().Throw<FormatException>();
    }

    private static TestCaseData[] InvalidTypes => new[]
    {
        new TestCaseData("PREFIX") { TestName = "Type can't have any uppercase letters" },
        new TestCaseData("pre_fix") { TestName = "Type can't have any underscores" },
        new TestCaseData("pre.fix") { TestName = "Type can't have any special characters" },
        new TestCaseData("pre fix") { TestName = "Type can't have any spaces" },
        new TestCaseData("préfix") { TestName = "Type can't have any non-ASCII characters" },
        new TestCaseData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijkl") { TestName = "Type can't have have more than 63 characters" },
    };
}