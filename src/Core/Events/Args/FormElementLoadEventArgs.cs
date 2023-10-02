using System;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.Events.Args;

public class FormElementLoadEventArgs : EventArgs
{
    public FormElement FormElement { get; }
    
    public FormElementLoadEventArgs(FormElement formElement)
    {
        FormElement = formElement;
    }
}