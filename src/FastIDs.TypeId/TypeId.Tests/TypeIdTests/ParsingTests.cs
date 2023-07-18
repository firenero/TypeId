using System;
using FastIDs.TypeId;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class ParsingTests
{
    [TestCaseSource(typeof(TestCases), nameof(TestCases.NoPrefix))]
    public void Parse_NoPrefixTypeId_Parsed(string typeIdStr, Guid expectedGuid)
    {
        var typeId = TypeId.Parse(typeIdStr);

        typeId.Type.Should().BeEmpty();
        typeId.Id.Should().Be(expectedGuid);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.WithPrefix))]
    public void Parse_WithPrefixTypeId_Parsed(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);

        typeId.Type.Should().Be(expectedType);
        typeId.Id.Should().Be(expectedGuid);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.InvalidIds))]
    public void Parse_InvalidTypeId_ThrowsFormatException(string typeIdStr)
    {
        Action act = () => TypeId.Parse(typeIdStr);
        
        act.Should().Throw<FormatException>();
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.NoPrefix))]
    public void TryParse_NoPrefixTypeId_Parsed(string typeIdStr, Guid expectedGuid)
    {
        var canParse = TypeId.TryParse(typeIdStr, out var typeId);
        
        canParse.Should().BeTrue();
        typeId.Type.Should().BeEmpty();
        typeId.Id.Should().Be(expectedGuid);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.WithPrefix))]
    public void TryParse_WithPrefixTypeId_Parsed(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var canParse = TypeId.TryParse(typeIdStr, out var typeId);
        
        canParse.Should().BeTrue();
        typeId.Type.Should().Be(expectedType);
        typeId.Id.Should().Be(expectedGuid);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.InvalidIds))]
    public void TryParse_InvalidTypeId_ReturnsFalse(string typeIdStr)
    {
        var canParse = TypeId.TryParse(typeIdStr, out var typeId);
        
        canParse.Should().BeFalse();
        typeId.Should().Be(default(TypeId));
    }
}