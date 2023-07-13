# TypeId

[![NuGet Version](https://img.shields.io/nuget/v/FastIDs.TypeId)](https://www.nuget.org/packages/FastIDs.TypeId)

High-performance C# implementation of [TypeId](https://github.com/jetpack-io/typeid/).

Here's an example of a TypeID of type user:
```
  user_2x4y6z8a0b1c2d3e4f5g6h7j8k
  └──┘ └────────────────────────┘
  type    uuid suffix (base32)
```

## Installation

Install from NuGet: https://www.nuget.org/packages/FastIDs.TypeId

## Benchmarks

This implementation is comparable or faster (sometimes 3x faster) in all common scenarios than other .NET implementations. 

See [Benchmarks wiki](https://github.com/firenero/TypeId/wiki/Benchmarks) for more details.

## Usage

### Import

```csharp
using FastIDs.TypeId;
```

### Creating a TypeId

Generate new ID:
```csharp
var typeId = TypeId.New("prefix");
```

It's also possible to create a TypeID without a prefix by passing the empty string:
```csharp
var typeIdWithoutPrefix = TypeId.New("");
```
*Note: If the prefix is empty, the separator `_` is omitted in the string representation.*

Create TypeId from existing UUIDv7:
```csharp
Guid uuidV7 = new Guid("01890a5d-ac96-774b-bcce-b302099a8057");
var typeId = TypeId.FromUuidV7("prefix", uuidV7);
```

Both `TypeId.New(prefix)` and `TypeId.FromUuidV7(prefix, guid)` validate provided prefix. 
You can skip this validation by using overloads with `bool validateType` parameter set to `false`.
The best case to do so is when you're 100% sure your prefix is correct and you want to squeeze extra bits of performance.
```csharp
var typeId = TypeId.New("prefix", false) // skips validation and creates a valid TypeId instance.

var invalidTypeId = TypeId.New("123", false) // doesn't throw FormatException despite invalid type provided
var shouldThrowId = TypeId.New("123") // throws FormatException
```

### Serialization to string

Get string representation of TypeId:
```csharp
var typeId = TypeId.FromUuidV7("type", new Guid("01890a5d-ac96-774b-bcce-b302099a8057"));
Console.WriteLine(typeId.ToString());
// prints "type_01h455vb4pex5vsknk084sn02q" (without quotes)
```

It's also possible to get only the suffix:
```csharp
var typeId = TypeId.FromUuidV7("type", new Guid("01890a5d-ac96-774b-bcce-b302099a8057"));
Console.WriteLine(typeId.GetSuffix());
// prints "01h455vb4pex5vsknk084sn02q" (without quotes)
```

*Note: if TypeID doesn't have a type (i.e. type is an empty string), values returned from `GetSuffix()` and `ToString()` are equal.*
```csharp
var typeId = TypeId.FromUuidV7("", new Guid("01890a5d-ac96-774b-bcce-b302099a8057"));
Console.WriteLine($"{typeId.GetSuffix()} == {typeId.ToString()}");
// prints: "01h455vb4pex5vsknk084sn02q == 01h455vb4pex5vsknk084sn02q" (without quotes)
```

### Parsing
Parse existing string representation to the `TypeId` instance:
```csharp
var typeId = TypeId.Parse("type_01h455vb4pex5vsknk084sn02q");
```

The `Parse(string input)` method will throw a `FormatException` in case of incorrect format of the passed value. Use the `TryParse(string input, out TypeId result)` method to avoid throwing the exception:
```csharp
if (TypeId.TryParse("type_01h455vb4pex5vsknk084sn02q", out var typeId))
    // TypeId is successfully parsed here.
else
    // Unable to parse TypeId from the provided string.
```

### Equality

`TypeId` struct implements the `IEquatable<TypeId>` interface with all its benefits:
* `typeId.Equals(other)` or `typeId == other` to check if IDs are same.
* `!typeId.Equals(other)` or `typeId != other` to check if IDs are different.
* Use `TypeId` as a key in `Dictionary` or `HashSet`. 
