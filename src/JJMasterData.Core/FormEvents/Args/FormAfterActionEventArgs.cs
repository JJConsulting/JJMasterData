using System;
using System.Collections;

namespace JJMasterData.Core.FormEvents.Args;

public class FormAfterActionEventArgs : EventArgs
{
    /// <summary>
    /// Campos do formulário, composto por um hash com o nome e valor do campo
    /// </summary>
    public Hashtable Values { get; set; }

    public string UrlRedirect { get; set; }

    public FormAfterActionEventArgs()
    {
        Values = new Hashtable();
    }

    public FormAfterActionEventArgs(Hashtable values)
    {
        Values = values;
    }

}
