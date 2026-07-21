using System.Collections.Generic;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public record PluginActionContext
{
    public required BasicAction Action { get; init; }
    public required FormElement FormElement { get; init; }
    public required FormStateData FormStateData { get; init; }
    public required string ParentComponentName { get; init; }
    public string? FieldName { get; init; }
    public required Dictionary<string,object?> ConfigurationMap { get; init; }
    public Dictionary<string, object?> Values => FormStateData.Values;
    public Dictionary<string, object?> SecretValues { get; } = new();

    public string Id
    {
        get
        {
            if (!DataHelper.ContainsPkValues(FormElement, Values))
                return FormElement.Name + "-" + Action.Name;

            var pkHash = DictionaryHash.ComputeHash(DataHelper.GetPkValues(FormElement, Values));
            return FormElement.Name + "-" + Action.Name + "-" + pkHash;
        }
    }
}
