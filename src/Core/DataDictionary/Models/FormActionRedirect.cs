using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Data.Entity.Models;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;


public class FormActionRedirect
{
    [JsonProperty("elementNameRedirect")]
    public string ElementNameRedirect { get; set; }

    [JsonProperty("entityReferences")]
    public List<FormActionRelationField> RelationFields { get; private init; } = [];

    [JsonProperty("viewType")]
    public RelationshipViewType ViewType { get; set; }

    [JsonProperty("popupSize")]
    public ModalSize ModalSize { get; set; }

    public FormActionRedirect DeepCopy()
    {
        return new()
        {
            ElementNameRedirect = ElementNameRedirect,
            ModalSize = ModalSize,
            ViewType = ViewType,
            RelationFields =  RelationFields.Select(s=>s.DeepCopy()).ToList(),
        };
    }
}