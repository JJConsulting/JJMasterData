using System;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Relationship data
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class ElementRelationColumn
{
    [DataMember(Name = "pkcolumn")]
    public string PkColumn { get; set; }

    [DataMember(Name = "fkcolumn")]
    public string FkColumn { get; set; }


    public ElementRelationColumn()
    {

    }

    public ElementRelationColumn(string pkColumn, string fkColumn)
    {
        PkColumn = pkColumn;
        FkColumn = fkColumn;
    }

}