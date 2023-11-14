using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Events.Args;

public class FormBeforeActionEventArgs(IDictionary<string, object> values, IDictionary<string, string> errors)
    : EventArgs
{
    public IDictionary<string, object> Values { get; set; } = values;
    public IDictionary<string, string> Errors { get; set; } = errors;
}
