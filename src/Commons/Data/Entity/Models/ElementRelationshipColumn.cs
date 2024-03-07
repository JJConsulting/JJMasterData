using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Relationship data
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementRelationshipColumn
{
    [JsonProperty("pkcolumn")]
    public string PkColumn { get; set; }

    [JsonProperty("fkcolumn")]
    public string FkColumn { get; set; }


    public ElementRelationshipColumn()
    {

    }

    public ElementRelationshipColumn(string pkColumn, string fkColumn)
    {
        PkColumn = pkColumn;
        FkColumn = fkColumn;
    }

    public ElementRelationshipColumn DeepCopy()
    {
        return new ElementRelationshipColumn()
        {
            FkColumn = FkColumn,
            PkColumn = PkColumn
        };
    }
}