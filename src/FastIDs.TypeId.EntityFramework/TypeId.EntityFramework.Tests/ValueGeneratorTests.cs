using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FastIDs.TypeId.EntityFramework.Tests;

[TestFixture]
public class ValueGeneratorTests
{
    [Test]
    public void TypeIdGenerator_GeneratesDistinctSortableIdsOfType()
    {
        var generator = new TypeIdValueGenerator("user");

        var ids = Enumerable.Range(0, 100).Select(_ => generator.Next(null!)).ToList();

        ids.Should().OnlyHaveUniqueItems();
        ids.Should().OnlyContain(id => id.HasType("user"));
        ids.Should().BeInAscendingOrder();
        generator.GeneratesTemporaryValues.Should().BeFalse();
    }

    [Test]
    public void TypeIdDecodedGenerator_GeneratesDistinctIdsOfType()
    {
        var generator = new TypeIdDecodedValueGenerator("acc");

        var ids = Enumerable.Range(0, 100).Select(_ => generator.Next(null!)).ToList();

        ids.Should().OnlyHaveUniqueItems();
        ids.Should().OnlyContain(id => id.HasType("acc"));
        generator.GeneratesTemporaryValues.Should().BeFalse();
    }

    [Test]
    public void Generators_InvalidType_ThrowInConstructor()
    {
        var act1 = () => new TypeIdValueGenerator("Bad Type");
        var act2 = () => new TypeIdDecodedValueGenerator("Bad Type");

        act1.Should().Throw<FormatException>();
        act2.Should().Throw<FormatException>();
    }

    [Test]
    public void AddingEntityWithDefaultKey_GeneratesKey()
    {
        using var factory = new SqliteContextFactory();
        using var context = factory.CreateContext();
        var user = new User();
        var account = new Account();

        context.Add(user);
        context.Add(account);

        // Generated when the entity is tracked, before SaveChanges.
        user.Id.HasType("user").Should().BeTrue();
        account.Id.HasType("acc").Should().BeTrue();
    }

    [Test]
    public void AddingEntityWithExplicitKey_KeepsKey()
    {
        using var factory = new SqliteContextFactory();
        using var context = factory.CreateContext();
        var explicitId = TypeId.New("user").Encode();
        var user = new User { Id = explicitId };

        context.Add(user);
        context.SaveChanges();

        user.Id.Should().Be(explicitId);
    }

    [Test]
    public void AddingMultipleEntities_GeneratesUniqueKeys()
    {
        using var factory = new SqliteContextFactory();
        using var context = factory.CreateContext();
        var users = Enumerable.Range(0, 20).Select(_ => new User()).ToList();

        context.Users.AddRange(users);
        context.SaveChanges();

        users.Select(u => u.Id).Should().OnlyHaveUniqueItems();
        context.Users.Count().Should().Be(20);
    }
}
