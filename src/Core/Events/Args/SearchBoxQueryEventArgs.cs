using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.Events.Args;

public class SearchBoxQueryEventArgs(string textSearch) : EventArgs
{
    public string TextSearch { get; set; } = textSearch;

    public IEnumerable<DataItemValue> Values { get; set; }
}
