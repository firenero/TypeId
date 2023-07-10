using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UUIDNext;

namespace FastIDs.TypeId;

[StructLayout(LayoutKind.Auto)]
public readonly struct TypeId : IEquatable<TypeId>
{
    public string Type { get; }
    
    public Guid Id { get; }

    private TypeId(string type, Guid id)
    {
        Type = type;
        Id = id;
    }

    public static TypeId New(string type) => FromUuidV7(type, Uuid.NewSequential());

    public static TypeId FromUuidV7(string type, Guid uuidV7)
    {
        if (type.Length > TypeIdConstants.MaxTypeLength)
            throw new FormatException($"Type can be at most {TypeIdConstants.MaxTypeLength} characters long.");
        if (!ValidateTypeAlphabet(type))
            throw new FormatException("Type must contain only lowercase letters.");

        return new TypeId(type, uuidV7);
    }

    public string GetSuffix()
    {
        Span<byte> idBytes = stackalloc byte[16];
        Id.TryWriteBytes(idBytes);

        FormatUuidBytes(idBytes);
        
        return Base32.Encode(idBytes);
    }

    public override string ToString() => Type.Length > 0 ? $"{Type}_{GetSuffix()}" : GetSuffix();

    public static TypeId Parse(string input)
    {
        var separatorIdx = input.IndexOf('_');
        if (separatorIdx == 0)
            throw new FormatException("Type separator must be omitted if there is no type present.");
        if (separatorIdx > TypeIdConstants.MaxTypeLength)
            throw new FormatException($"Type can be at most {TypeIdConstants.MaxTypeLength} characters long.");

        var typeSpan = separatorIdx != -1 ? input.AsSpan(0, separatorIdx) : ReadOnlySpan<char>.Empty;
        if (!ValidateTypeAlphabet(typeSpan))
            throw new FormatException("Type must contain only lowercase letters.");

        var idSpan = input.AsSpan(separatorIdx + 1);
        if (idSpan.Length != TypeIdConstants.IdLength)
            throw new FormatException($"Id must be {TypeIdConstants.IdLength} characters long.");
        if (idSpan[0] > '7')
            throw new FormatException("The maximum possible suffix for TypeId is '7zzzzzzzzzzzzzzzzzzzzzzzzz'");
        
        Span<byte> decoded = stackalloc byte[Base32Constants.DecodedLength];
        if (!Base32.TryDecode(idSpan, decoded))
            throw new FormatException("Id is not a valid Base32 string.");
 
        FormatUuidBytes(decoded);

        return new TypeId(typeSpan.ToString(), new Guid(decoded));
    }

    public static bool TryParse(string input, out TypeId result)
    {
        var separatorIdx = input.IndexOf('_');
        if (separatorIdx is 0 or > TypeIdConstants.MaxTypeLength)
            return Error(out result);

        var typeSpan = separatorIdx != -1 ? input.AsSpan(0, separatorIdx) : ReadOnlySpan<char>.Empty;
        if (!ValidateTypeAlphabet(typeSpan))
            return Error(out result);

        var idSpan = input.AsSpan(separatorIdx + 1);
        if (idSpan.Length != TypeIdConstants.IdLength || idSpan[0] > '7')
            return Error(out result);

        Span<byte> decoded = stackalloc byte[Base32Constants.DecodedLength];
        if (!Base32.TryDecode(idSpan, decoded))
            return Error(out result);
        
        FormatUuidBytes(decoded);

        result = new TypeId(typeSpan.ToString(), new Guid(decoded));
        return true;
    }

    public bool Equals(TypeId other) => Type == other.Type && Id.Equals(other.Id);

    public override bool Equals(object? obj) => obj is TypeId other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Type, Id);

    public static bool operator ==(TypeId left, TypeId right) => left.Equals(right);

    public static bool operator !=(TypeId left, TypeId right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Error(out TypeId result)
    {
        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ValidateTypeAlphabet(ReadOnlySpan<char> type)
    {
        foreach (var c in type)
        {
            if (c is < 'a' or > 'z')
                return false;
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FormatUuidBytes(Span<byte> bytes)
    {
        bytes[..4].Reverse();
        (bytes[4], bytes[5]) = (bytes[5], bytes[4]);
        (bytes[6], bytes[7]) = (bytes[7], bytes[6]);
    }
}