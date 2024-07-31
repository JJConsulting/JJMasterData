#nullable enable

using System.Linq;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components;

public record ActionContext
{
    public required BasicAction Action { get; init; }
    public required FormElement FormElement { get; init; }
    public required FormStateData FormStateData { get; init; }
    public required string ParentComponentName { get; init; }
    public required bool IsSubmit { get; init; }

    public string? FieldName { get; init; }
    
    internal ActionMap ToActionMap(ActionSource actionSource)
    {
        var actionMap = new ActionMap
        {
            ActionName = Action.Name,
            ElementName = FormElement.Name,
            ActionSource = actionSource,
            FieldName = FieldName
        };

        if (FormStateData.Values.Count == 0)
            return actionMap;
        
        var values = FormStateData.Values;
        if(DataHelper.ContainsPkValues(FormElement,values))
            actionMap.PkFieldValues = DataHelper.GetPkValues(FormElement, values);

        return actionMap;
    }
}