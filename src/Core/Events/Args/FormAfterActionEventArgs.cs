using System;
using System.Collections;
using System.Collections.Generic;

namespace JJMasterData.Core.FormEvents.Args;

public class FormAfterActionEventArgs : EventArgs
{
    public IDictionary<string,dynamic> Values { get; set; }

    public string UrlRedirect { get; set; }

    public FormAfterActionEventArgs()
    {
        Values = new Dictionary<string,dynamic>();
    }

    public FormAfterActionEventArgs(IDictionary<string,dynamic>values)
    {
        Values = values;
    }

}
