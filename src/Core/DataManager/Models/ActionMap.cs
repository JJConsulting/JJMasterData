#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager;

public class ActionMap
{
    [JsonProperty("elementName")]
    public required string ElementName { get; set; }

    [JsonProperty("actionName")] 
    public required string ActionName { get; set; }

    [JsonProperty("fieldName")] 
    public string? FieldName { get; set; }

    [JsonProperty("pkFieldValues")] 
    public IDictionary<string, object> PkFieldValues { get; set; }
    
    
    [JsonProperty("contextAction")] 
    public required ActionSource ActionSource { get; set; }

    public ActionMap()
    {
        PkFieldValues = new Dictionary<string, object>();
    }
    
    [SetsRequiredMembers]
    public ActionMap(
        ActionSource actionSource,
        FormElement formElement,
        IDictionary<string, object> row,
        string actionName)
    {
        ElementName = formElement.Name;
        ActionSource = actionSource;
        ActionName = actionName;
        PkFieldValues = new Dictionary<string, object>();
        foreach (var f in formElement.Fields.ToList().FindAll(x => x.IsPk)
                     .Where(f => row.ContainsKey(f.Name) && row[f.Name] != null))
        {
            PkFieldValues.Add(f.Name, row[f.Name].ToString()!);
        }
    }
    
    internal BasicAction GetCurrentAction(FormElement formElement)
    {
        return ActionSource switch
        {
            ActionSource.GridTable => formElement.Options.GridTableActions.First(a => a.Name.Equals(ActionName)),
            ActionSource.GridToolbar =>  formElement.Options.GridToolbarActions.First(a => a.Name.Equals(ActionName)),
            ActionSource.FormToolbar =>  formElement.Options.FormToolbarActions.First(a => a.Name.Equals(ActionName)),
            ActionSource.Field => formElement.Fields[FieldName!].Actions.Get(ActionName),
            _ => throw new JJMasterDataException("Invalid ActionSource"),
        };
    }
}