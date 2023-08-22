using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;


public class FormActionRedirect
{
    [JsonProperty("elementNameRedirect")]
    public string ElementNameRedirect { get; set; }= null!;

    [JsonProperty("entityReferences")]
    public List<FormActionRelationField> RelationFields { get; set; }

    [JsonProperty("viewType")]
    public RelationshipViewType ViewType { get; set; }

    [JsonProperty("popupSize")]
    public PopupSize PopupSize { get; set; }
    
    public FormActionRedirect()
    {
        RelationFields = new List<FormActionRelationField>();
    }

        
}