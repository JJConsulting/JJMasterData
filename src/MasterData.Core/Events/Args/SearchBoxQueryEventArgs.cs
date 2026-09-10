#nullable disable warnings
using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.Events.Args;

public class SearchBoxQueryEventArgs(string? searchText) : EventArgs
{
    public string? SearchText { get; set; } = searchText;

    public IEnumerable<DataItemValue> Values { get; set; }
}
