using System;
using System.Collections;

namespace JJMasterData.Core.FormEvents.Args;

public class FormAfterActionEventArgs : EventArgs
{
    public IDictionary Values { get; set; }

    public string UrlRedirect { get; set; }

    public FormAfterActionEventArgs()
    {
        Values = new Hashtable();
    }

    public FormAfterActionEventArgs(IDictionary values)
    {
        Values = values;
    }

}
