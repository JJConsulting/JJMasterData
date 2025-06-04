#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Serialization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;


namespace JJMasterData.Core.UI.Components;

public class ActionMap
{

    [JsonPropertyName("elementName")]
    public required string ElementName { get; set; }

    [JsonPropertyName("actionName")] 
    public required string ActionName { get; set; }

    [JsonPropertyName("fieldName")] 
    public string? FieldName { get; set; }

    [JsonPropertyName("pkFieldValues")] 
    [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
    public Dictionary<string, object> PkFieldValues { get; set; }
    
    [JsonPropertyName("contextAction")] 
    public required ActionSource ActionSource { get; set; }

    public ActionMap()
    {
        PkFieldValues = new Dictionary<string, object>();
    }
    
    [SetsRequiredMembers]
    public ActionMap(
        ActionSource actionSource,
        FormElement formElement,
        Dictionary<string, object?> row,
        string actionName)
    {
        ElementName = formElement.Name;
        ActionSource = actionSource;
        ActionName = actionName;
        PkFieldValues = new Dictionary<string, object>();
        foreach (var f in formElement.Fields)
        {
            if (!f.IsPk) 
                continue;
            
            if (row.TryGetValue(f.Name, out var value) && value != null)
            {
                PkFieldValues.Add(f.Name, value.ToString()!);
            }
        }
    }
    
    internal BasicAction? GetAction(FormElement formElement)
    {
        return GetAction<BasicAction>(formElement);
    }
    
    internal TAction? GetAction<TAction>(FormElement formElement) where TAction : BasicAction
    {
        var action = ActionSource switch
        {
            ActionSource.GridTable => formElement.Options.GridTableActions.GetOrDefault<TAction>(ActionName),
            ActionSource.GridToolbar => formElement.Options.GridToolbarActions.GetOrDefault<TAction>(ActionName),
            ActionSource.FormToolbar => formElement.Options.FormToolbarActions.GetOrDefault<TAction>(ActionName),
            ActionSource.Field => formElement.Fields[FieldName!].Actions.GetOrDefault<TAction>(ActionName),
            _ => throw new JJMasterDataException("Invalid ActionSource")
        };

        return action;
    }
}