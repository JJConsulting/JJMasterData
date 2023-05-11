using System;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Relationship data
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class ElementRelationshipColumn
{
    [DataMember(Name = "pkcolumn")]
    public string PkColumn { get; set; }

    [DataMember(Name = "fkcolumn")]
    public string FkColumn { get; set; }


    public ElementRelationshipColumn()
    {

    }

    public ElementRelationshipColumn(string pkColumn, string fkColumn)
    {
        PkColumn = pkColumn;
        FkColumn = fkColumn;
    }

}