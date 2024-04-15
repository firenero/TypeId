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

    [TestCaseSource(nameof(ValidTypes))]
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

    private static TestCaseData[] ValidTypes =>
    [
        new("prefix") { TestName = "Lowercase letters type" },
        new("pre_fix") { TestName = "Lowercase letters with underscore type" },
        new("") { TestName = "Empty type" },
    ];

    private static TestCaseData[] InvalidTypes =>
    [
        new("PREFIX") { TestName = "Type can't have any uppercase letters" },
        new("pre.fix") { TestName = "Type can't have any special characters" },
        new("pre fix") { TestName = "Type can't have any spaces" },
        new("préfix") { TestName = "Type can't have any non-ASCII characters" },
        new("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijkl") { TestName = "Type can't have have more than 63 characters" },
        new("_prefix") { TestName = "The prefix can't start with an underscore" },
        new("prefix_") { TestName = "The prefix can't end with an underscore" },
    ];
}