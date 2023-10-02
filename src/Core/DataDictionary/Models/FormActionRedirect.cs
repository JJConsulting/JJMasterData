using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Models;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;


public class FormActionRedirect
{
    [JsonProperty("elementNameRedirect")]
    public string ElementNameRedirect { get; set; }

    [JsonProperty("entityReferences")]
    public List<FormActionRelationField> RelationFields { get; set; }

    [JsonProperty("viewType")]
    public RelationshipViewType ViewType { get; set; }

    [JsonProperty("popupSize")]
    public ModalSize ModalSize { get; set; }
    
    public FormActionRedirect()
    {
        RelationFields = new List<FormActionRelationField>();
    }

        
}