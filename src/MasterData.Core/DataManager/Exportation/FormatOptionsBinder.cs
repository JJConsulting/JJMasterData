using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace JJMasterData.Core.DataManager.Exportation;

internal static class FormatOptionsBinder
{
    public static TOptions Bind<TOptions>(
        List<ExportFormatOption>? definitions,
        Dictionary<string, string?> suppliedValues) where TOptions : class, new()
    {
        var definitionMap = (definitions ?? []).ToDictionary(
            definition => definition.Name, StringComparer.OrdinalIgnoreCase);
        var values = definitionMap.Values
            .Where(option => option.DefaultValue is not null)
            .ToDictionary(option => option.Name, option => option.DefaultValue, StringComparer.OrdinalIgnoreCase);
        foreach (var option in suppliedValues)
        {
            if (!definitionMap.TryGetValue(option.Key, out var definition))
                throw new InvalidOperationException($"Option '{option.Key}' is not supported.");
            if (definition.Kind == ExportFormatOptionKind.Select &&
                definition.Choices?.Any(choice => choice.Value == option.Value) is false)
                throw new InvalidOperationException($"Value '{option.Value}' is invalid for option '{option.Key}'.");
            values[option.Key] = option.Value;
        }
        return Bind<TOptions>(values);
    }

    public static TOptions Bind<TOptions>(Dictionary<string, string?> values)
        where TOptions : class, new()
    {
        var result = new TOptions();
        foreach (var property in typeof(TOptions).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanWrite || !values.TryGetValue(property.Name, out var value))
                continue;
            property.SetValue(result, ConvertValue(property.PropertyType, value));
        }
        return result;
    }

    private static object? ConvertValue(Type targetType, string? value)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (value is null && Nullable.GetUnderlyingType(targetType) is not null)
            return null;
        if (underlyingType == typeof(string))
            return value;
        if (underlyingType.IsEnum)
            return Enum.Parse(underlyingType, value!, true);

        var converter = TypeDescriptor.GetConverter(underlyingType);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value ?? string.Empty);
        throw new InvalidOperationException($"Option type '{targetType.Name}' is not supported.");
    }
}
