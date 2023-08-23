#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Models;

public class ActionContext
{
    public required FormElement FormElement { get; init; }
    public required FormStateData FormStateData { get; init; }
    public required string? ParentComponentName { get; init; }
    public bool IsExternalRoute { get; init; }
    public string? FieldName { get; init; }
    
    public static async Task<ActionContext> FromFormViewAsync(JJFormView formView)
    {
        return new ActionContext
        {
            FormElement = formView.FormElement,
            FormStateData = await formView.GetFormStateDataAsync(),
            ParentComponentName = formView.Name,
            IsExternalRoute = formView.IsExternalRoute
        };
    }
    
    public static ActionContext FromGridView(JJGridView gridView, FormStateData formStateData)
    {
        return new ActionContext
        {
            FormElement = gridView.FormElement,
            FormStateData = formStateData,
            ParentComponentName = gridView.Name,
            IsExternalRoute = gridView.IsExternalRoute
        };
    }
    
    public ActionMap ToActionMap(string actionName, ActionSource actionSource)
    {
        var actionMap = new ActionMap
        {
            ActionName = actionName,
            DictionaryName = FormElement.Name,
            ActionSource = actionSource,
            FieldName = FieldName
        };

        if (FormStateData.UserValues is not null)
            actionMap.UserValues = FormStateData.UserValues;

        if (!FormStateData.FormValues.Any()) 
            return actionMap;
        
        var values = FormStateData.FormValues;
        if(DataHelper.ContainsPkValues(FormElement,values))
            actionMap.PkFieldValues = DataHelper.GetPkValues(FormElement, values);

        return actionMap;
    }
}