using System;
using System.Linq;
using FastIDs.TypeId;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class FormattingTests
{
    [TestCaseSource(typeof(TestCases),nameof(TestCases.WithPrefix))]
    public void GetSuffix_WithPrefix_SuffixReturned(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);

        var suffix = typeId.GetSuffix();

        var expectedSuffix = typeIdStr.Split('_')[1];
        suffix.Should().Be(expectedSuffix);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.NoPrefix))]
    public void GetSuffix_NoPrefix_SuffixReturned(string typeIdStr, Guid expectedGuid)
    {
        var typeId = TypeId.Parse(typeIdStr);

        var suffix = typeId.GetSuffix();

        suffix.Should().Be(typeIdStr);
    }
    
    [TestCaseSource(nameof(ToStringTestCases))]
    public void ToString_StringReturned(string typeIdString)
    {
        var typeId = TypeId.Parse(typeIdString);

        typeId.ToString().Should().Be(typeIdString);
    }

    private static TestCaseData[] ToStringTestCases => TestCases.WithPrefix
        .Concat(TestCases.NoPrefix)
        .Select(x => new TestCaseData(x.OriginalArguments[0]))
        .ToArray();
}