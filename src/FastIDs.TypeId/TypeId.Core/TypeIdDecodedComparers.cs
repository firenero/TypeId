using FastIDs.TypeId.Uuid;

namespace FastIDs.TypeId;

public class TypeIdDecodedLexComparer : IComparer<TypeIdDecoded>
{
    public static TypeIdDecodedLexComparer Instance { get; } = new();
    
    public int Compare(TypeIdDecoded x, TypeIdDecoded y)
    {
        var typeComparison = string.CompareOrdinal(x.Type, y.Type);
        if (typeComparison != 0) 
            return typeComparison;

        return UuidComparer.Instance.Compare(x.Id, y.Id);
    }
}

public class TypeIdDecodedTimestampComparer : IComparer<TypeIdDecoded>
{
    public static TypeIdDecodedTimestampComparer Instance { get; } = new();
    
    public int Compare(TypeIdDecoded x, TypeIdDecoded y)
    {
        var idComparison = UuidComparer.Instance.Compare(x.Id, y.Id);
        if (idComparison != 0)
            return idComparison;

        return string.CompareOrdinal(x.Type, y.Type);
    }
}