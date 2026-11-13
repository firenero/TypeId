using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FastIDs.TypeId.EntityFramework.Tests;

/// <summary>
/// Configures properties with the converters and comparers directly, without <c>ConfigureForTypeId</c>.
/// </summary>
[TestFixture]
public class ExplicitConversionTests
{
    public class Parent
    {
        public TypeId Id { get; set; }
        public List<Child> Children { get; set; } = new();
    }

    public class Child
    {
        public TypeIdDecoded Id { get; set; }
        public TypeId ParentId { get; set; }
        public Parent? Parent { get; set; }
    }

    private class ExplicitContext : DbContext
    {
        public ExplicitContext(DbContextOptions<ExplicitContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>().Property(x => x.Id)
                .HasConversion<TypeIdToStringConverter, TypeIdValueComparer>()
                .HasTypeIdGenerator("parent");

            modelBuilder.Entity<Child>(b =>
            {
                b.Property(x => x.Id)
                    .HasConversion<TypeIdDecodedToStringConverter, TypeIdDecodedValueComparer>()
                    .HasTypeIdGenerator("child");
                b.Property(x => x.ParentId).HasConversion<TypeIdToStringConverter, TypeIdValueComparer>();
                b.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId);
            });
        }
    }

    [Test]
    public void AttachingGraphWithUnpopulatedForeignKey_Works()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ExplicitContext>().UseSqlite(connection).Options;
        var parent = new Parent();
        parent.Children.Add(new Child());
        parent.Children.Add(new Child());

        using (var context = new ExplicitContext(options))
        {
            context.Database.EnsureCreated();
            context.Add(parent);
            context.SaveChanges();
        }

        using (var context = new ExplicitContext(options))
        {
            var loaded = context.Set<Parent>().Include(p => p.Children).Single();
            loaded.Id.Should().Be(parent.Id);
            loaded.Children.Select(c => c.Id).Should().BeEquivalentTo(parent.Children.Select(c => c.Id));
            loaded.Children.Should().OnlyContain(c => c.ParentId == parent.Id);
        }
    }
}
