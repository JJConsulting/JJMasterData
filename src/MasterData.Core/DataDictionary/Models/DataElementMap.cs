#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace JJMasterData.Core.DataDictionary.Models;

public class DataElementMap
{
    [JsonPropertyName("elementName")]
    [Required]
    public required string ElementName { get; set; } = null!;

    [JsonPropertyName("fieldKey")] 
    public string IdFieldName { get; set; } = null!;

    [JsonPropertyName("fieldDescription")]
    public string? DescriptionFieldName { get; set; } = null!;

    [JsonPropertyName("iconId")]
    public string? IconIdFieldName { get; set; }

    [JsonPropertyName("iconColor")] 
    public string? IconColorFieldName { get; set; }

    [JsonPropertyName("group")]
    public string? GroupFieldName { get; set; }

    [JsonPropertyName("popUpSize")] 
    public ModalSize ModalSize { get; set; }

    [JsonIgnore]
    public Dictionary<string, object> Filters
    {
        get
        {
            var filters = new Dictionary<string, object>();


            foreach (var item in MapFilters ?? [])
                filters.Add(item.FieldName, item.ExpressionValue);

            return filters;
        }
        set
        {
            MapFilters.Clear();
            foreach (var s in value)
            {
                var mapFilter = new DataElementMapFilter
                {
                    FieldName = s.Key,
                    ExpressionValue = s.ToString()
                };
                MapFilters.Add(mapFilter);
            }
        }
    }

    [JsonPropertyName("mapFilters")]
    public List<DataElementMapFilter> MapFilters { get; set; } = [];


    [JsonPropertyName("enableElementActions")]
    [Display(Name = "Enable Element Actions")]
    public bool EnableElementActions { get; set; }

    public DataElementMap DeepCopy()
    {
        var copy = (DataElementMap)MemberwiseClone();
        
        copy.MapFilters = MapFilters.ConvertAll(m => m.DeepCopy());
        
        return copy;
    }
}