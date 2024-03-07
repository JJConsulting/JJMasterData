using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Specific relationship information between tables
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementRelationship
{
    [JsonProperty("childElement")]
    [Display(Name = "Child Element")]
    public string ChildElement { get; set; }
        
    [JsonProperty("columns")]
    public List<ElementRelationshipColumn> Columns { get; set; }

    [JsonProperty("updateOnCascade")]
    [Display(Name = "Update On Cascade")]
    public bool UpdateOnCascade { get; set; }

    [JsonProperty("deleteOnCascade")]
    [Display(Name = "Delete On Cascade")]
    public bool DeleteOnCascade { get; set; }

    public ElementRelationship()
    {
        Columns = [];
    }

    public ElementRelationship(string childElement, params ElementRelationshipColumn[] columns)
    {
        Columns = [];
        ChildElement = childElement;
        if (columns != null)
            Columns.AddRange(columns);
    }

    public ElementRelationship DeepCopy()
    {
        var copy = (ElementRelationship)MemberwiseClone();
        copy.Columns = Columns.ConvertAll(c => c.DeepCopy());
        return copy;
    }
}