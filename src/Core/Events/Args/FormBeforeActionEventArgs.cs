using System;
using System.Collections.Generic;

namespace JJMasterData.Core.FormEvents.Args;

public class FormBeforeActionEventArgs : EventArgs
{
    public IDictionary<string, object> Values { get; set; }
    public IDictionary<string, string> Errors { get; set; }

    public FormBeforeActionEventArgs(IDictionary<string, object> values, IDictionary<string, string> errors)
    {
        Values = values;
        Errors = errors;
    }

}
