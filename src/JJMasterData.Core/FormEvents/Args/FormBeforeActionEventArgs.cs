using System;
using System.Collections;

namespace JJMasterData.Core.FormEvents.Args;

public class FormBeforeActionEventArgs : EventArgs
{
    public IDictionary Values { get; set; }
    public IDictionary Errors { get; set; }

    public FormBeforeActionEventArgs(IDictionary values, IDictionary errors)
    {
        Values = values;
        Errors = errors;
    }

}
