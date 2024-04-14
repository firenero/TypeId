using System;
using FastIDs.TypeId;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class ParsingTests
{
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Parse_ValidIds_Parsed(string typeIdStr, Guid expectedGuid, string expectedPrefix)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        typeId.ToString().Should().Be(typeIdStr);
        typeId.Type.ToString().Should().Be(expectedPrefix);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void TryParse_ValidIds_Parsed(string typeIdStr, Guid expectedGuid, string expectedPrefix)
    {
        var canParse = TypeId.TryParse(typeIdStr, out var typeId);
        
        canParse.Should().BeTrue();
        typeId.ToString().Should().Be(typeIdStr);
        typeId.Type.ToString().Should().Be(expectedPrefix);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.InvalidIds))]
    public void Parse_InvalidId_ThrowsFormatException(string typeIdStr)
    {
        FluentActions.Invoking(() => TypeId.Parse(typeIdStr))
            .Should().Throw<FormatException>();
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.InvalidIds))]
    public void TryParse_InvalidId_ReturnsFalse(string typeIdStr)
    {
        var canParse = TypeId.TryParse(typeIdStr, out var typeId);
        
        canParse.Should().BeFalse();
        typeId.Should().Be((TypeId)default);
    }
}