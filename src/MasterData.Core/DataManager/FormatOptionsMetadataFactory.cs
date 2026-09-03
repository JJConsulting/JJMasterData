using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace JJMasterData.Core.DataManager;

internal static class FormatOptionsMetadataFactory
{
    internal static IReadOnlyList<FormatOptionMetadata> CreateOptions(FormatOptions defaults)
    {
        return defaults.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.GetMethod?.IsPublic == true && property.SetMethod?.IsPublic == true &&
                               property.GetIndexParameters().Length == 0)
            .Select(property => CreateOption(property, defaults))
            .ToArray();
    }

    internal static string GetEnumValue(FieldInfo field) =>
        field.GetCustomAttribute<DisplayAttribute>()?.ShortName is { Length: > 0 } shortName
            ? shortName
            : field.Name;

    private static FormatOptionMetadata CreateOption(PropertyInfo property, FormatOptions defaults)
    {
        var displayName = property.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? property.Name;
        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var defaultValue = ConvertToString(propertyType, property.GetValue(defaults));

        if (propertyType == typeof(bool))
            return new FormatOptionMetadata(property.Name, displayName, FormatOptionKind.Boolean, defaultValue, []);

        if (propertyType.IsEnum)
        {
            var choices = propertyType.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field => new FormatOptionChoiceMetadata(
                    GetEnumValue(field),
                    field.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? field.Name))
                .ToArray();
            return new FormatOptionMetadata(property.Name, displayName, FormatOptionKind.Select, defaultValue, choices);
        }

        var converter = TypeDescriptor.GetConverter(propertyType);
        if (propertyType != typeof(string) && !converter.CanConvertFrom(typeof(string)))
            throw new InvalidOperationException(
                $"Option property '{defaults.GetType().Name}.{property.Name}' has unsupported type '{property.PropertyType.Name}'.");

        return new FormatOptionMetadata(property.Name, displayName, FormatOptionKind.Input, defaultValue, []);
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
