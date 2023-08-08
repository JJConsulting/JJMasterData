using System;
using System.Collections;
using System.Collections.Generic;

namespace JJMasterData.Core.FormEvents.Args;

public class FormBeforeActionEventArgs : EventArgs
{
    public IDictionary<string,dynamic> Values { get; set; }
    public IDictionary<string,dynamic> Errors { get; set; }

    public FormBeforeActionEventArgs(IDictionary<string,dynamic> values, IDictionary<string,dynamic> errors)
    {
        Values = values;
        Errors = errors;
    }

}
