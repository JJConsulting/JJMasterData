#nullable enable
using JJConsulting.FontAwesome;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager;

public static class FontAwesomeIconHelper
{
    internal static FontAwesomeIcon GetFontAwesomeIconFromField(FormElementField field, object value)
    {
        if (value is int intValue)
            return (FontAwesomeIcon)intValue;

        return GetFontAwesomeIconFromField(field, value.ToString());
    }
    
    public static FontAwesomeIcon GetFontAwesomeIconFromField(FormElementField field, string? value)
    {
        if (int.TryParse(value, out var parsedInt))
            return (FontAwesomeIcon)parsedInt;
        
        throw new JJMasterDataException($"Invalid IconType id at {field.LabelOrName}.");
    }
}