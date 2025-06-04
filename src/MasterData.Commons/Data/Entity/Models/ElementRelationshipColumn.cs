

using System.Text.Json.Serialization;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Relationship data
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementRelationshipColumn
{
    [JsonPropertyName("pkcolumn")]
    public string PkColumn { get; set; }

    [JsonPropertyName("fkcolumn")]
    public string FkColumn { get; set; }


    public ElementRelationshipColumn()
    {

    }

    public ElementRelationshipColumn(string pkColumn, string fkColumn)
    {
        PkColumn = pkColumn;
        FkColumn = fkColumn;
    }

    public ElementRelationshipColumn DeepCopy() => (ElementRelationshipColumn)MemberwiseClone();
}