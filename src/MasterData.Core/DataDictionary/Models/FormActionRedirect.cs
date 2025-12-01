using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJConsulting.Html.Bootstrap.Models;
using JJMasterData.Commons.Data.Entity.Models;


namespace JJMasterData.Core.DataDictionary.Models;


public class FormActionRedirect
{
    [JsonPropertyName("elementNameRedirect")]
    public string ElementNameRedirect { get; set; }

    [JsonPropertyName("entityReferences")]
    [JsonInclude]
    public List<FormActionRelationField> RelationFields { get; private set; } = [];

    [JsonPropertyName("viewType")]
    public RelationshipViewType ViewType { get; set; }

    [JsonPropertyName("popupSize")]
    public ModalSize ModalSize { get; set; }

    [JsonPropertyName("showAsModal")]
    [Display(Name = "Show as Modal")]
    public bool ShowAsModal { get; set; } = true;
    
    public FormActionRedirect DeepCopy()
    {
        var copy = (FormActionRedirect)MemberwiseClone();
        copy.RelationFields = RelationFields.ConvertAll(s => s.DeepCopy());

        return copy;
    }
}