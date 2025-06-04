using System;
using System.Collections.Generic;

namespace JJMasterData.Core.Events.Args;

public class FormAfterActionEventArgs(Dictionary<string, object> values) : EventArgs
{
    public Dictionary<string, object> Values { get; set; } = values;

    public string UrlRedirect { get; set; }

    public FormAfterActionEventArgs() : this(new Dictionary<string, object>())
    {
    }
}
