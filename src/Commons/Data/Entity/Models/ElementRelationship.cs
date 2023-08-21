using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Specific relationship information between tables
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementRelationship
{
    [JsonProperty("childElement")]
    public string ChildElement { get; set; }
        
    [JsonProperty("columns")]
    public List<ElementRelationshipColumn> Columns { get; set; }

    [JsonProperty("updateOnCascade")]
    public bool UpdateOnCascade { get; set; }

    [JsonProperty("deleteOnCascade")]
    public bool DeleteOnCascade { get; set; }

    public ElementRelationship()
    {
        Columns = new List<ElementRelationshipColumn>();
    }

    public ElementRelationship(string childElement, params ElementRelationshipColumn[] columns)
    {
        Columns = new List<ElementRelationshipColumn>();
        ChildElement = childElement;
        if (columns != null)
            Columns.AddRange(columns);
    }

}