using System;
using FastIDs.TypeId;
using FluentAssertions;

namespace TypeIdTests.TypeIdTests;

[TestFixture]
public class TimestampTests
{
    [Test]
    public void GetTimestamp_NewlyGenerated()
    {
        var typeId = TypeId.New("", false);
        
        var timestamp = typeId.GetTimestamp();

        timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(100));
    }

    [Test]
    public void GetTimestamp_ExistingId()
    {
        var typeId = TypeId.FromUuidV7("", new Guid("01896af3-a83a-7155-bf7e-fff6e73fe09d"), false);
        
        var timestamp = typeId.GetTimestamp();
        var expectedTimestamp = new DateTimeOffset(2023, 07, 18, 21, 41, 40, 538, TimeSpan.Zero);

        timestamp.Should().Be(expectedTimestamp);
    }
}