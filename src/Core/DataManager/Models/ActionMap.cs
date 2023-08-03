using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager;

public class ActionMap
{
    [JsonProperty("dictionaryName")]
    public required string DictionaryName { get; set; }

    [JsonProperty("actionName")] 
    public required string ActionName { get; set; }

    [JsonProperty("fieldName")] 
    public string FieldName { get; set; }

    [JsonProperty("pkFieldValues")] 
    public IDictionary<string, dynamic> PkFieldValues { get; }

    [JsonProperty("contextAction")] 
    public required ActionSource ActionSource { get; set; }

    public ActionMap()
    {
        PkFieldValues = new Dictionary<string, dynamic>();
    }
    
    [SetsRequiredMembers]
    public ActionMap(
        ActionSource actionSource,
        FormElement formElement,
        IDictionary<string, dynamic> row,
        string actionName)
    {
        DictionaryName = formElement.Name;
        ActionSource = actionSource;
        ActionName = actionName;
        PkFieldValues = new Dictionary<string, dynamic>();
        foreach (var f in formElement.Fields.ToList().FindAll(x => x.IsPk)
                     .Where(f => row.ContainsKey(f.Name) && row[f.Name] != null))
        {
            PkFieldValues.Add(f.Name, row[f.Name].ToString());
        }
    }
}