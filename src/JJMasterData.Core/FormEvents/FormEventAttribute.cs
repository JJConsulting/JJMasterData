using System;

namespace JJMasterData.Core.FormEvents;

public class FormEventAttribute : Attribute
{
    public string ElementName { get; }
    
    public FormEventAttribute(string elementName)
    {
        ElementName = elementName;
    }
}