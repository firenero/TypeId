using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastIDs.TypeId.EntityFramework.Tests;

[TestFixture]
public class DatabaseRoundTripTests
{
    private SqliteContextFactory _factory = null!;

    [SetUp]
    public void SetUp() => _factory = new SqliteContextFactory();

    [TearDown]
    public void TearDown() => _factory.Dispose();

    private object? ScalarRaw(string sql)
    {
        using var command = _factory.Connection.CreateCommand();
        command.CommandText = sql;
        var result = command.ExecuteScalar();
        return result is DBNull ? null : result;
    }

    [Test]
    public void TypeId_StoredAsReadableString_AndReadBack()
    {
        var id = TypeId.New("user").Encode();
        using (var context = _factory.CreateContext())
        {
            context.Users.Add(new User { Id = id, Name = "Ann" });
            context.SaveChanges();
        }

        ScalarRaw("SELECT Id FROM Users").Should().Be(id.ToString());

        using (var context = _factory.CreateContext())
        {
            var user = context.Users.Single();
            user.Id.Should().Be(id);
            user.ReferrerId.Should().BeNull();
            user.Name.Should().Be("Ann");
        }
    }

    [Test]
    public void TypeIdDecoded_StoredAsReadableString_AndReadBack()
    {
        var id = TypeId.New("acc");
        using (var context = _factory.CreateContext())
        {
            context.Accounts.Add(new Account { Id = id, Name = "Main" });
            context.SaveChanges();
        }

        ScalarRaw("SELECT Id FROM Accounts").Should().Be(id.ToString());

        using (var context = _factory.CreateContext())
        {
            var account = context.Accounts.Single();
            account.Id.Should().Be(id);
            account.Id.Type.Should().Be("acc");
            account.ParentId.Should().BeNull();
        }
    }

    [Test]
    public void TypeId_WithGuidConversion_StoredAsUuid_AndReadBack()
    {
        var user = new User { Name = "Ann" };
        var order = new Order { User = user };
        using (var context = _factory.CreateContext())
        {
            context.Add(order);
            context.SaveChanges();
        }

        order.Id.HasType("order").Should().BeTrue();
        var stored = Guid.Parse((string)ScalarRaw("SELECT Id FROM Orders")!);
        stored.Should().Be(order.Id.Decode().Id);

        using (var context = _factory.CreateContext())
        {
            var loaded = context.Orders.Single();
            loaded.Id.Should().Be(order.Id);
            loaded.Id.ToString().Should().StartWith("order_");
            loaded.UserId.Should().Be(user.Id);
        }
    }

    [Test]
    public void TypeIdDecoded_WithGuidConversion_StoredAsUuid_AndReadBack()
    {
        var invoice = new Invoice();
        using (var context = _factory.CreateContext())
        {
            context.Invoices.Add(invoice);
            context.SaveChanges();
        }

        invoice.Id.Type.Should().Be("inv");
        Guid.Parse((string)ScalarRaw("SELECT Id FROM Invoices")!).Should().Be(invoice.Id.Id);

        using (var context = _factory.CreateContext())
        {
            context.Invoices.Single().Id.Should().Be(invoice.Id);
        }
    }

    [Test]
    public void TypeId_EmptyType_RoundTrips()
    {
        var withoutType = TypeId.FromUuidV7("", Guid.NewGuid()).Encode();
        using (var context = _factory.CreateContext())
        {
            context.Manuals.Add(new Manual { Id = withoutType, Decoded = TypeId.FromUuidV7("", Guid.NewGuid()) });
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            var manual = context.Manuals.Single();
            manual.Id.Should().Be(withoutType);
            manual.Decoded.Type.Should().BeEmpty();
        }
    }

    [Test]
    public void Where_FilterByTypeId_UsesConverterForParameter()
    {
        var target = TypeId.New("user").Encode();
        using (var context = _factory.CreateContext())
        {
            context.Users.AddRange(new User { Name = "other" }, new User { Id = target, Name = "target" }, new User { Name = "other2" });
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            context.Users.Where(u => u.Id == target).Select(u => u.Name).Single().Should().Be("target");
            context.Users.Count(u => u.Id != target).Should().Be(2);
            var ids = new[] { target };
            context.Users.Count(u => ids.Contains(u.Id)).Should().Be(1);
        }
    }

    [Test]
    public void Where_FilterByTypeIdDecoded_UsesConverterForParameter()
    {
        var target = TypeId.New("acc");
        using (var context = _factory.CreateContext())
        {
            context.Accounts.AddRange(new Account { Name = "other" }, new Account { Id = target, Name = "target" });
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            context.Accounts.Where(a => a.Id == target).Select(a => a.Name).Single().Should().Be("target");
        }
    }

    [Test]
    public void Where_FilterByGuidMappedTypeId_UsesConverterForParameter()
    {
        var user = new User { Name = "Ann" };
        var order1 = new Order { User = user };
        var order2 = new Order { User = user };
        using (var context = _factory.CreateContext())
        {
            context.AddRange(order1, order2);
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            context.Orders.Single(o => o.Id == order2.Id).Id.Should().Be(order2.Id);
        }
    }

    [Test]
    public void Find_ByTypeId_Works()
    {
        var user = new User { Name = "Ann" };
        var order = new Order { User = user };
        using (var context = _factory.CreateContext())
        {
            context.Add(order);
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            context.Users.Find(user.Id)!.Name.Should().Be("Ann");
            context.Orders.Find(order.Id).Should().NotBeNull();
        }
    }

    [Test]
    public void NullableTypeId_SelfReference_RoundTrips()
    {
        var referrer = new User { Name = "referrer" };
        var referred = new User { Name = "referred", ReferrerId = referrer.Id };
        using (var context = _factory.CreateContext())
        {
            context.Users.Add(referrer);
            context.SaveChanges();
            referred.ReferrerId = referrer.Id;
            context.Users.Add(referred);
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            context.Users.Single(u => u.Name == "referred").ReferrerId.Should().Be(referrer.Id);
            context.Users.Single(u => u.Name == "referrer").ReferrerId.Should().BeNull();
            context.Users.Count(u => u.ReferrerId == null).Should().Be(1);
            context.Users.Count(u => u.ReferrerId == referrer.Id).Should().Be(1);
        }
    }

    [Test]
    public void NullableTypeIdDecoded_SelfReference_RoundTrips()
    {
        var parent = new Account { Name = "parent" };
        using (var context = _factory.CreateContext())
        {
            context.Accounts.Add(parent);
            context.SaveChanges();
            context.Accounts.Add(new Account { Name = "child", ParentId = parent.Id });
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            context.Accounts.Single(a => a.Name == "child").ParentId.Should().Be(parent.Id);
            context.Accounts.Single(a => a.Name == "parent").ParentId.Should().BeNull();
        }
    }

    [Test]
    public void ForeignKey_Include_LoadsRelatedEntities()
    {
        var user = new User { Name = "Ann" };
        user.Orders.Add(new Order());
        user.Orders.Add(new Order());
        using (var context = _factory.CreateContext())
        {
            context.Users.Add(user);
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            var loaded = context.Users.Include(u => u.Orders).Single();
            loaded.Orders.Select(o => o.Id).Should().BeEquivalentTo(user.Orders.Select(o => o.Id));
            loaded.Orders.Should().OnlyContain(o => o.UserId == user.Id);
        }
    }

    [Test]
    public void ProjectionAndOrdering_OfTypeIds_Work()
    {
        var ids = Enumerable.Range(0, 5).Select(_ => TypeId.New("acc")).ToArray();
        using (var context = _factory.CreateContext())
        {
            context.Accounts.AddRange(ids.Select(id => new Account { Id = id }));
            context.SaveChanges();
        }

        using (var context = _factory.CreateContext())
        {
            var loaded = context.Accounts.OrderBy(a => a.Id).Select(a => a.Id).ToList();
            loaded.Should().BeEquivalentTo(ids);
            // Fixed-type string IDs are K-sortable by creation time (default SQLite collation is binary).
            loaded.Should().BeInAscendingOrder(TypeIdDecoded.Comparers.Lex);
        }
    }

    [Test]
    public void ReadingInvalidStoredValue_Throws()
    {
        using (var command = _factory.Connection.CreateCommand())
        {
            command.CommandText = "INSERT INTO Users (Id, Name) VALUES ('not a type id', 'bad')";
            command.ExecuteNonQuery();
        }

        using var context = _factory.CreateContext();
        var act = () => context.Users.ToList();

        act.Should().Throw<FormatException>();
    }

    [Test]
    public void WritingGuidMappedTypeIdOfWrongType_Throws()
    {
        using var context = _factory.CreateContext();
        context.Manuals.Add(new Manual
        {
            Id = TypeId.New("x").Encode(),
            Decoded = TypeId.New("wrong")
        });

        var act = () => context.SaveChanges();

        act.Should().Throw<DbUpdateException>()
            .WithInnerException<InvalidOperationException>().WithMessage("*'wrong_*'*''*");
    }
}
