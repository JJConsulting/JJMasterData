using System;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Core.DataDictionary.Exceptions;

public class FormElementNotFoundException : JJMasterDataException
{
    public string ElementName { get; }
    
    public FormElementNotFoundException(string elementName) : base($"Element {elementName} was not found.")
    {
        ElementName = elementName;
    }

    public FormElementNotFoundException(string elementName, Exception innerException)
        : base($"Element {elementName} was not found.", innerException)
    {
        ElementName = elementName;
    }
}
