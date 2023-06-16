using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Extensions;

public static class FormElementExtensions
{
    public static IDictionary<string, dynamic?> GetPrimaryKeyValues(this FormElement formElement, IDictionary<string, dynamic?> values)
    {
        foreach (var entry in values)
        {
            if (formElement.Fields.Contains(entry.Key) && !formElement.Fields[entry.Key].IsPk)
            {
                values.Remove(entry);
            }
        }

        return values;
    }
}