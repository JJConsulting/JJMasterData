#nullable enable

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementFieldSelector(FormElement formElement, string fieldName)
{
    public FormElement FormElement { get; } = formElement;
    public FormElementField Field { get; } = formElement.Fields[fieldName];
}