using System;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.FormEvents.Args;

public class FormEventLoadEventArgs : EventArgs
{
    public FormElement FormElement { get; }
    
    public FormEventLoadEventArgs(FormElement formElement)
    {
        FormElement = formElement;
    }
}