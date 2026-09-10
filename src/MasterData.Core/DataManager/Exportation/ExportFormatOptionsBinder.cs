using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation;

internal static class ExportFormatOptionsBinder
{
    public static ExportFormatOptions Bind(
        IReadOnlyList<ExportFormatOptionMetadata> definitions,
        Type type,
        Dictionary<string, string?> suppliedValues)
    {
        var definitionMap = new Dictionary<string, ExportFormatOptionMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in definitions)
            definitionMap.Add(definition.Name, definition);

        var result = (ExportFormatOptions)Activator.CreateInstance(type)!;
        foreach (var option in suppliedValues)
        {
            if (!definitionMap.TryGetValue(option.Key, out var definition))
                throw new InvalidOperationException($"Option '{option.Key}' is not supported.");
            var property = type.GetProperty(definition.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)!;
            property.SetValue(result, ConvertValue(property.PropertyType, option.Value, definition));
        }
        return result;
    }

    private static object? ConvertValue(Type targetType, string? value, ExportFormatOptionMetadata definition)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (value is null && Nullable.GetUnderlyingType(targetType) is not null)
            return null;
        if (underlyingType == typeof(string))
            return value;
        if (underlyingType.IsEnum)
        {
            foreach (var field in underlyingType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (string.Equals(ExportFormatOptionsMetadataFactory.GetEnumValue(field), value,
                        StringComparison.OrdinalIgnoreCase))
                    return field.GetValue(null);
            }
            throw new InvalidOperationException($"Value '{value}' is invalid for option '{definition.Name}'.");
        }

        var converter = TypeDescriptor.GetConverter(underlyingType);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value ?? string.Empty);
        throw new InvalidOperationException($"Option type '{targetType.Name}' is not supported.");
    }
}
