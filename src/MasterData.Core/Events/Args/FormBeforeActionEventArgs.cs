using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Events.Args;

public class FormBeforeActionEventArgs(Dictionary<string, object> values, Dictionary<string, string> errors)
    : EventArgs
{
    public Dictionary<string, object> Values { get; set; } = values;
    public Dictionary<string, string> Errors { get; set; } = errors;
    
}
