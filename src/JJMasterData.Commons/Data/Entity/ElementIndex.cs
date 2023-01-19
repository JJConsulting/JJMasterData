using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Index specific information
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class ElementIndex
{
    [DataMember(Name = "columns")]
    public List<string> Columns { get; set; }

    [DataMember(Name = "isunique")]
    public bool IsUnique { get; set; }

    [DataMember(Name = "isclustered")]
    public bool IsClustered { get; set; }

    public ElementIndex()
    {
        Columns = new List<string>();
    }

    public ElementIndex(bool isUnique, params string[] columns)
    {
        IsUnique = isUnique;
        Columns = columns.ToList();
    }
}