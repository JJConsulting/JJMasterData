using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.Events.Args;

public class SearchBoxQueryEventArgs : EventArgs
{
    public string TextSearch { get; set; }

    public IEnumerable<DataItemValue> Values { get; set; }

    public SearchBoxQueryEventArgs(string textSearch)
    {
        TextSearch = textSearch;
    }
}
