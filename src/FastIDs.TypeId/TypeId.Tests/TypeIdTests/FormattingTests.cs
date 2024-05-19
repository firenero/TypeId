using System;
using System.Linq;
using System.Text;
using System.Text.Unicode;
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
    public void Encoded_ToStringFormat(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);

        typeId.ToString("", null).Should().Be(typeIdStr);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_TryFormat(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        Span<char> formattedTypeId = stackalloc char[typeIdStr.Length + 10];
        typeId.TryFormat(formattedTypeId, out var charsWritten, "", null).Should().BeTrue();

        formattedTypeId[..charsWritten].ToString().Should().Be(typeIdStr);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Encoded_TryFormatUtf8(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var typeId = TypeId.Parse(typeIdStr);
        
        Span<byte> formattedTypeId = stackalloc byte[typeIdStr.Length + 10];
        typeId.TryFormat(formattedTypeId, out var bytesWritten, "", null).Should().BeTrue();

        Encoding.UTF8.GetString(formattedTypeId[..bytesWritten]).Should().Be(typeIdStr);
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
    public void Decoded_GetSuffixUtf8Span(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);
        
        var expectedSuffix = typeIdStr.Split('_')[^1];
    
        Span<byte> utf8Suffix = stackalloc byte[expectedSuffix.Length + 10];
        var bytesWritten = decoded.GetSuffix(utf8Suffix);
        
        bytesWritten.Should().Be(expectedSuffix.Length);
        var suffix = Encoding.UTF8.GetString(utf8Suffix[..bytesWritten]);
        suffix.Should().Be(expectedSuffix);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_ToString(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);

        decoded.ToString().Should().Be(typeIdStr);
    }

    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_ToStringFormat(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);

        decoded.ToString("", null).Should().Be(typeIdStr);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_TryFormat(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);

        Span<char> formattedTypeId = stackalloc char[typeIdStr.Length + 10];
        decoded.TryFormat(formattedTypeId, out var charsWritten, "", null).Should().BeTrue();

        formattedTypeId[..charsWritten].ToString().Should().Be(typeIdStr);
    }
    
    [TestCaseSource(typeof(TestCases), nameof(TestCases.ValidIds))]
    public void Decoded_TryFormatUtf8(string typeIdStr, Guid expectedGuid, string expectedType)
    {
        var decoded = TypeId.FromUuidV7(expectedType, expectedGuid);

        Span<byte> formattedTypeId = stackalloc byte[typeIdStr.Length + 10];
        decoded.TryFormat(formattedTypeId, out var bytesWritten, "", null).Should().BeTrue();

        Encoding.UTF8.GetString(formattedTypeId[..bytesWritten]).Should().Be(typeIdStr);
    }
}