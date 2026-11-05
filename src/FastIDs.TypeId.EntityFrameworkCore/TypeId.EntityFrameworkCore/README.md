# FastIDs.TypeId.EntityFrameworkCore

[Entity Framework Core](https://learn.microsoft.com/ef/core/) integration for [FastIDs.TypeId](https://www.nuget.org/packages/FastIDs.TypeId):
value converters, a model convention and client-side value generators for `TypeId` and `TypeIdDecoded`.

Requires EF Core 8.0 or later.

## Installation

```
dotnet add package FastIDs.TypeId.EntityFrameworkCore
```

## Quick start

Register the convention once. Every `TypeId` / `TypeIdDecoded` property (nullable ones too) in the model is then
mapped to a `varchar(90)`-like column automatically:

```csharp
using FastIDs.TypeId;
using FastIDs.TypeId.EntityFrameworkCore;

public class MyContext : DbContext
{
    public DbSet<User> Users => Set<User>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        => configurationBuilder.ConfigureForTypeId();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<User>().Property(u => u.Id).HasTypeIdGenerator("user");
}

public class User
{
    public TypeId Id { get; set; }
    public string Name { get; set; } = "";
}
```

`HasTypeIdGenerator("user")` generates a new UUIDv7-based ID on the client whenever an entity with a default `Id` is
added, so `context.Users.Add(new User { Name = "Ann" })` just works.

## Storage options

| Mapping | Column | Types per column | Notes |
|---|---|---|---|
| `ConfigureForTypeId()` (default) | string, max length 90, non-unicode | any | Zero-copy write for `TypeId` (it already holds the string). Values keep the readable `user_...` form in the database. |
| `HasTypeIdGuidConversion("user")` | native UUID | exactly one, fixed in the mapping | 16 bytes, smallest and fastest index. Writing a `TypeId` of another type throws instead of silently changing it. |

Mix them freely, the property-level mapping wins over the convention:

```csharp
modelBuilder.Entity<Order>().Property(o => o.Id)
    .HasTypeIdGuidConversion("order")
    .HasTypeIdGenerator("order");
```

You can also use the converters directly: `TypeIdToStringConverter`, `TypeIdDecodedToStringConverter`,
`TypeIdToGuidConverter`, `TypeIdDecodedToGuidConverter`.

### Which one to use?

* The string mapping is the safest default: self-describing values, any type in any column, works on every provider.
  `TypeId` is the cheapest type to use with it because converting it to and from a string doesn't allocate.
* Prefer the UUID mapping when you have a single type per column and care about index size and insert speed. 
  UUIDv7 is time-ordered, which keeps B-tree inserts append-mostly on PostgreSQL and MySQL.
  SQL Server sorts `uniqueidentifier` values in a different byte order, so IDs are not sorted by creation time there.
* String columns are compared using the database collation. If you sort or range-query by a string TypeId column,
  use a binary/ordinal collation to get ordinal ordering consistent with `TypeId.CompareTo`.
