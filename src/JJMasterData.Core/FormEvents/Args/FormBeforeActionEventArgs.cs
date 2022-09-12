using System;
using System.Collections;

namespace JJMasterData.Core.FormEvents.Args;

public class FormBeforeActionEventArgs : EventArgs
{
    /// <summary>
    /// All fields, the key is the field name and the value itself.
    /// </summary>
    public Hashtable Values { get; set; }

    /// <summary>
    /// Fields with errors, the key is the field and the description the error details.
    /// </summary>
    public Hashtable Errors { get; set; }

    public FormBeforeActionEventArgs(Hashtable values, Hashtable errors)
    {
        Values = values;
        Errors = errors;
    }

}
