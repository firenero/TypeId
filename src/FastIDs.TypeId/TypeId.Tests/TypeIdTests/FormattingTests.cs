using System;
using System.Linq;
using FastIDs.TypeId;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class FormattingTests
{
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_Suffix(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        var suffix = typeId.Suffix.ToString();
        var expectedSuffix = typeIdStr.Split('_')[^1];

        suffix.Should().Be(expectedSuffix);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_Type(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        var type = typeId.Type.ToString();

        type.Should().Be(expectedType);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_ToString(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);

        typeId.ToString().Should().Be(typeIdStr);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_GetSuffix(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);
        
        var expectedSuffix = typeIdStr.Split('_')[^1];

        decoded.GetSuffix().Should().Be(expectedSuffix);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_GetSuffixSpan(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);
        
        var expectedSuffix = typeIdStr.Split('_')[^1];

        Span<char> suffix = stackalloc char[expectedSuffix.Length + 10];
        var charsWritten = decoded.GetSuffix(suffix);
        
        charsWritten.Should().Be(expectedSuffix.Length);
        suffix[..charsWritten].ToString().Should().Be(expectedSuffix);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_ToString(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);

        decoded.ToString().Should().Be(typeIdStr);
    }
}