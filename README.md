# TypeId

[![NuGet Version](https://img.shields.io/nuget/v/FastIDs.TypeId)](https://www.nuget.org/packages/FastIDs.TypeId)

High-performance C# implementation of [TypeId](https://github.com/jetpack-io/typeid/).

Here's an example of a TypeID of type user:
```
  user_2x4y6z8a0b1c2d3e4f5g6h7j8k
  └──┘ └────────────────────────┘
  type    uuid suffix (base32)
```

## Why another library?

This implementation is comparable or faster (sometimes 3x faster) in all common scenarios than other .NET implementations. 
It also allocates up to 5x less memory, reducing GC pressure.

See the [Benchmarks wiki](https://github.com/firenero/TypeId/wiki/Benchmarks) for more details.

### Why should you care?

You may think that generating, parsing, or serializing a single TypeId is very fast regardless of the implementation. 
To some degree, that's true. But small inefficiencies accumulate quickly, and they are very hard to spot in a large system. 
Most likely, there are millions of IDs parsed and serialized across your whole application daily. 
There is no single place with "slow" performance to spot in the profiler, so it's very hard to notice these inefficiencies.

GC is another important factor.
If every small library generates tons of short-lived objects for no reason, the GC will trigger much more frequently, impacting your whole application. 
The tricky part? There is nothing you can do about it because the memory is allocated inside 3rd party code.

There is no reason to use inefficient building blocks in your application. 
With this library, you get the same high-level, easy-to-use API. 
You don't have to deal with "weird" performance-oriented approaches. 
However, I plan to expose additional performance-oriented APIs in the near future for those who need them.

## Installation

Install from NuGet: https://www.nuget.org/packages/FastIDs.TypeId

## Usage

The library exposes two types optimized for slightly different use-cases. For better understanding refer to [TypeId vs TypeIdDecoded](https://github.com/firenero/TypeId/wiki/Choosing-the-right-type:-TypeId-vs.-TypeIdDecoded) section in wiki. 

This readme covers the basic operation you need to know to use the library.

### Import

```csharp
using FastIDs.TypeId;
```

### Creating a TypeId

TypeId can be generated using static methods of `TypeId` and `TypeIdDecoded` classes. Both have the same API and will create an instance of `TypeIdDecoded` struct. Examples in this section only use `TypeId` for simplicity.

Generate new ID:
```csharp
var typeIdDecoded = TypeId.New("prefix");
```

It's also possible to create a TypeID without a prefix by passing the empty string:
```csharp
var typeIdDecodedWithoutPrefix = TypeId.New("");
```
*Note: If the prefix is empty, the separator `_` is omitted in the string representation.*

Create TypeId from existing UUIDv7:
```csharp
Guid uuidV7 = new Guid("01890a5d-ac96-774b-bcce-b302099a8057");
var typeIdDecoded = TypeId.FromUuidV7("prefix", uuidV7);
```

Both `TypeId.New(prefix)` and `TypeId.FromUuidV7(prefix, guid)` validate provided prefix. 
You can skip this validation by using overloads with `bool validateType` parameter set to `false`.
The best case to do so is when you're 100% sure your prefix is correct and you want to squeeze extra bits of performance.
```csharp
var typeIdDecoded = TypeId.New("prefix", false) // skips validation and creates a valid TypeId instance.

var invalidTypeIdDecoded = TypeId.New("123", false) // doesn't throw FormatException despite invalid type provided
var shouldThrowIdDecoded = TypeId.New("123") // throws FormatException
```

### Conversion between `TypeId` and `TypeIdDecoded`

Convert `TypeIdDecoded` to `TypeId`:
```csharp
TypeId typeId = typeIdDecoded.Encode();
```

Convert `TypeId` to `TypeIdDecoded`:
```csharp
TypeIdDecoded typeIdDecoded = typeId.Decode();
```

### `TypeId` serialization to string

All the following examples assume that there is a `typeId` variable created this way:
```csharp
TypeId typeId = TypeId.FromUuidV7("type", new Guid("01890a5d-ac96-774b-bcce-b302099a8057")).Encode();
```

Get string representation
```csharp
string typeIdString = typeId.ToString();
// returns "type_01h455vb4pex5vsknk084sn02q" (without quotes)
```

It's possible to get only type:
```csharp
ReadOnlySpan<char> typeSpan = typeId.Type;
string type = typeSpan.ToString();
// both are "type" (without quotes)
```

It's also possible to get only the suffix:
```csharp
ReadOnlySpan<char> suffixSpan = typeId.Suffix;
string suffix = suffixSpan.ToString();
// both are "01h455vb4pex5vsknk084sn02q" (without quotes)
```

*Note: if TypeID doesn't have a type (i.e. type is an empty string), values returned from `Suffix` and `ToString()` are equal.*
```csharp
var typeId = TypeId.FromUuidV7("", new Guid("01890a5d-ac96-774b-bcce-b302099a8057")).Encode();
Console.WriteLine($"{typeId.Suffix.ToString()} == {typeId.ToString()}");
// prints: "01h455vb4pex5vsknk084sn02q == 01h455vb4pex5vsknk084sn02q" (without quotes)
```


### `TypeIdDecoded` serialization to string

All the following examples assume that there is a `typeIdDecoded` variable created this way:
```csharp
TypeIdDecoded typeIdDecoded = TypeId.FromUuidV7("type", new Guid("01890a5d-ac96-774b-bcce-b302099a8057"));
```

Get string representation
```csharp
string typeIdString = typeIdDecoded.ToString();
// returns "type_01h455vb4pex5vsknk084sn02q" (without quotes)
```

It's possible to get only type:
```csharp
string type = typeIdDecoded.Type;
// returns "type" (without quotes)
```

It's also possible to get only the suffix:
```csharp
string suffix = typeIdDecoded.GetSuffix();
// returns "01h455vb4pex5vsknk084sn02q" (without quotes)

Span<char> suffixSpan = stackalloc char[26];
int charsWritten = typeIdDecoded.GetSuffix(suffixSpan);
// `charsWritten` is 26, and `suffixSpan` contains "01h455vb4pex5vsknk084sn02q" (without quotes)
```


*Note: if TypeID doesn't have a type (i.e. type is an empty string), values returned from `GetSuffix()` and `ToString()` are equal.*
```csharp
var typeIdDecoded = TypeId.FromUuidV7("", new Guid("01890a5d-ac96-774b-bcce-b302099a8057"));
Console.WriteLine($"{typeIdDecoded.GetSuffix()} == {typeIdDecoded.ToString()}");
// prints: "01h455vb4pex5vsknk084sn02q == 01h455vb4pex5vsknk084sn02q" (without quotes)
```

### Parsing

String representation can only be parsed into `TypeId`.

Parse existing string representation to the `TypeId` instance:
```csharp
TypeId typeId = TypeId.Parse("type_01h455vb4pex5vsknk084sn02q");
```

The `Parse(string input)` method will throw a `FormatException` in case of incorrect format of the passed value. Use the `TryParse(string input, out TypeId result)` method to avoid throwing the exception:
```csharp
if (TypeId.TryParse("type_01h455vb4pex5vsknk084sn02q", out TypeId typeId))
    // TypeId is successfully parsed here.
else
    // Unable to parse TypeId from the provided string.
```

### Match type

Both `TypeId` and `TypeIdDecoded` have the same API for checking if the type equals the provided value.

```csharp
bool isSameType = typeId.HasType("your_type"); // also has overload for ReadOnlySpan<char>
```

### Equality

Both `TypeId` and `TypeIdDecoded` structs implement the `IEquatable<T>` interface with all its benefits:
* `typeId.Equals(other)` or `typeId == other` to check if IDs are same.
* `!typeId.Equals(other)` or `typeId != other` to check if IDs are different.
* Use `TypeId` as a key in `Dictionary` or `HashSet`.

### UUIDv7 component operations

`TypeIdDecoded` provides the API for accessing the UUIDv7 component of the TypeID. 

Get `Guid`:
```csharp
Guid uuidv7 = typeIdDecoded.Id;
```

Get the creation timestamp (part of the UUIDv7 component):
```csharp
DateTimeOffset timestamp = typeIdDecoded.GetTimestamp();
```

### Json Serialization

NuGet packages are available for working with JSON. 

  - [System.Text.Json](https://www.nuget.org/packages/FastIDs.TypeId.Serialization.SystemTextJson)
  - [Json.Net](https://www.nuget.org/packages/FastIDs.TypeId.Serialization.SystemTextJson)

You can use the extension method `ConfigureForTypeId` on the `JsonSerializerSettings` type for `Json.Net` or on the `JsonSerializerOptions` type for `System.Text.Json` to automatically serialize a `TypeId` or a `TypeIdDecoded` to a string.

If you are using [SwashBuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore), you will need to configure your service as follows:

```cs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "api", Version = "v1" });

    c.MapType(typeof(TypeId), () => new OpenApiSchema { Type = "string", Example = new OpenApiString("prefix_01h93ech7jf5ktdwg6ye383x34") });
    c.MapType(typeof(TypeIdDecoded), () => new OpenApiSchema { Type = "string", Example = new OpenApiString("prefix_01h93ech7jf5ktdwg6ye383x34") });
});
```
