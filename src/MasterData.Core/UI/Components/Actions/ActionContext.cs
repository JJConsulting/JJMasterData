#nullable enable

using JJMasterData.Commons.Util;
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
    public bool IsSubmit => Action is ISubmittableAction { IsSubmit: true };
    public string? FieldName { get; init; }
    
    public string Id
    {
        get
        {
            var values = FormStateData.Values;


            if (DataHelper.ContainsPkValues(FormElement, values))
            {
                var pkValues = DataHelper.GetPkValues(FormElement, values);
                
                var pkHash = DictionaryHash.ComputeHash(pkValues);
        
                return FormElement.Name + "-" + Action.Name + "-" + pkHash;
            }

            return FormElement.Name + "-" + Action.Name;
        }
    }
    
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