﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Index specific information
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementIndex
{
    [JsonProperty("columns")] 
    public List<string> Columns { get; set; }

    [JsonProperty("isunique")] 
    public bool IsUnique { get; set; }

    [JsonProperty("isclustered")] 
    public bool IsClustered { get; set; }

    public ElementIndex()
    {
        Columns = [];
    }

    public ElementIndex(bool isUnique, params string[] columns)
    {
        IsUnique = isUnique;
        Columns = columns.ToList();
    }

    public ElementIndex DeepCopy()
    {
        var copy = (ElementIndex)MemberwiseClone();
        copy.Columns = [..Columns];
        return copy;
    }
}