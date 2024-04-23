#nullable enable

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementFieldSelector(FormElement formElement, string fieldName)
{
    public FormElement FormElement { get; set; } = formElement;
    public FormElementField Field => FormElement.Fields[fieldName];
}