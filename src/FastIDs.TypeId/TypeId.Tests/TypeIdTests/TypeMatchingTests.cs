using System;
using FastIDs.TypeId;
using FluentAssertions;
using NUnit.Framework;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class TypeMatchingTests
{
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_HasType_Valid(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        typeId.HasType(expectedType).Should().BeTrue();
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_HasType_WrongType(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        typeId.HasType(expectedType + '#').Should().BeFalse();
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_HasTypeSpan_Valid(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        typeId.HasType(expectedType.AsSpan()).Should().BeTrue();
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_HasType_Valid(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);
        
        decoded.HasType(expectedType).Should().BeTrue();
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_HasTypeSpan_Valid(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);

        decoded.HasType(expectedType.AsSpan()).Should().BeTrue();
    }
}