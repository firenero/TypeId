using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace FastIDs.TypeId;

internal class TypeIdTypeConverterBase : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    
    public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) => 
        destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
}

internal sealed class TypeIdTypeConverter : TypeIdTypeConverterBase
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? TypeId.Parse(text)
            : base.ConvertFrom(context, culture, value);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
        value is TypeId typeId
            ? typeId.ToString()
            : base.ConvertTo(context, culture, value, destinationType);
}

internal sealed class TypeIdDecodedTypeConverter : TypeIdTypeConverterBase
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
        value is string text
            ? TypeId.Parse(text).Decode()
            : base.ConvertFrom(context, culture, value);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
        value is TypeIdDecoded typeIdDecoded
            ? typeIdDecoded.ToString()
            : base.ConvertTo(context, culture, value, destinationType);
}