using System;

namespace JJMasterData.Core.Events.Args;

public class SearchBoxItemEventArgs : EventArgs
{
    public string IdSearch { get; set; }

    public string ResultText { get; set; }

    public SearchBoxItemEventArgs(string idSearch)
    {
        IdSearch = idSearch;
    }
}
