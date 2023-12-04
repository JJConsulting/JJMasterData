using System;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.Events.Args;

public class FormElementLoadEventArgs(FormElement formElement) : EventArgs
{
    public FormElement FormElement { get; } = formElement;
}