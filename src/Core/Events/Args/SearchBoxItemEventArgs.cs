using System;

namespace JJMasterData.Core.Events.Args;

public class SearchBoxItemEventArgs(string idSearch) : EventArgs
{
    public string IdSearch { get; set; } = idSearch;

    public string ResultText { get; set; }
}
