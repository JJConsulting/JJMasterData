using System;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.FormEvents.Args;

public class FormElementLoadEventArgs : EventArgs
{
    public FormElement FormElement { get; }
    
    public FormElementLoadEventArgs(FormElement formElement)
    {
        FormElement = formElement;
    }
}