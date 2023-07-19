using System;
using FastIDs.TypeId;
using FluentAssertions;
using NUnit.Framework;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class IdConvertTests
{
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decode_ValidIds(string typeIdStr, Guid expectedGuid, string expectedPrefix)
    {
        var typeId = TypeId.Parse(typeIdStr);
        var decoded = typeId.Decode();
        
        decoded.Type.Should().Be(expectedPrefix);
        decoded.Id.Should().Be(expectedGuid);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encode_ValidIds(string typeIdStr, Guid expectedGuid, string expectedPrefix)
    {
        var decoded = TypeId.FromUuidV7(expectedPrefix, expectedGuid);

        var typeId = decoded.Encode();

        typeId.ToString().Should().Be(typeIdStr);
    }
}