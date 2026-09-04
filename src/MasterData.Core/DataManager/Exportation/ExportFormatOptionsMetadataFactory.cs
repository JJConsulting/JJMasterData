using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation;

internal static class ExportFormatOptionsMetadataFactory
{
    internal static IReadOnlyList<ExportFormatOptionMetadata> CreateOptions(IExportFormat exportFormat)
    {
        var optionsType = exportFormat.OptionsType;

        var defaultOptionsValues = (ExportFormatOptions)Activator.CreateInstance(optionsType)!;
        
        return exportFormat.OptionsType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetMethod?.IsPublic == true && property.SetMethod?.IsPublic == true &&
                               property.GetIndexParameters().Length == 0)
            .Select(property => CreateOption(property, defaultOptionsValues))
            .ToArray();
    }

    internal static string GetEnumValue(FieldInfo field) =>
        field.GetCustomAttribute<DisplayAttribute>()?.ShortName is { Length: > 0 } shortName
            ? shortName
            : field.Name;

    private static ExportFormatOptionMetadata CreateOption(PropertyInfo property, ExportFormatOptions defaultOptions)
    {
        var displayName = property.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? property.Name;
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var defaultValue = ConvertToString(propertyType, property.GetValue(defaultOptions));

        if (propertyType == typeof(bool))
            return new ExportFormatOptionMetadata(property.Name, displayName, ExportFormatOptionKind.Boolean, defaultValue, []);

        if (propertyType.IsEnum)
        {
            var choices = propertyType.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field => new ExportFormatOptionChoiceMetadata(
                    GetEnumValue(field),
                    field.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? field.Name))
                .ToArray();
            return new ExportFormatOptionMetadata(property.Name, displayName, ExportFormatOptionKind.Select, defaultValue, choices);
        }

        var converter = TypeDescriptor.GetConverter(propertyType);
        if (propertyType != typeof(string) && !converter.CanConvertFrom(typeof(string)))
            throw new InvalidOperationException(
                $"Option property '{defaultOptions.GetType().Name}.{property.Name}' has unsupported type '{property.PropertyType.Name}'.");

        return new ExportFormatOptionMetadata(property.Name, displayName, ExportFormatOptionKind.Input, defaultValue, []);
    }

    private static string? ConvertToString(Type type, object? value)
    {
        if (value is null)
            return null;
        if (type == typeof(bool))
            return ((bool)value).ToString().ToLowerInvariant();
        if (type.IsEnum)
        {
            var field = type.GetField(value.ToString()!)!;
            return GetEnumValue(field);
        }
        if (type == typeof(string))
            return (string)value;

        return TypeDescriptor.GetConverter(type).ConvertToString(null, CultureInfo.InvariantCulture, value);
    }
}
