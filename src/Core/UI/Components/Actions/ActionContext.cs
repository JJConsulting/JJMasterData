#nullable enable

using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Models;

public class ActionContext
{
    public required FormElement FormElement { get; init; }
    public required FormStateData FormStateData { get; init; }
    public required string ParentComponentName { get; init; }
    public bool IsModal { get; set; }
    public string? FieldName { get; init; }
    public bool IsSubmit { get; set; }

    
    public static ActionContext FromFormView(JJFormView formView, FormStateData formStateData)
    {
        return new ActionContext
        {
            FormElement = formView.FormElement,
            FormStateData = formStateData,
            IsModal = formView.ComponentContext is ComponentContext.Modal,
            ParentComponentName = formView.Name
        };
    }
    
    public static async Task<ActionContext> FromFormViewAsync(JJFormView formView)
    {
        return new ActionContext
        {
            FormElement = formView.FormElement,
            FormStateData = await formView.GetFormStateDataAsync(),
            IsModal = formView.ComponentContext is ComponentContext.Modal,
            ParentComponentName = formView.Name
        };
    }
    
    public static ActionContext FromGridView(JJGridView gridView, FormStateData formStateData)
    {
        return new ActionContext
        {
            FormElement = gridView.FormElement,
            FormStateData = formStateData,
            ParentComponentName = gridView.Name,
            IsModal = gridView.ComponentContext is ComponentContext.Modal
        };
    }
    
    public ActionMap ToActionMap(string actionName, ActionSource actionSource)
    {
        var actionMap = new ActionMap
        {
            ActionName = actionName,
            ElementName = FormElement.Name,
            ActionSource = actionSource,
            FieldName = FieldName
        };

        if (!FormStateData.FormValues.Any()) 
            return actionMap;
        
        var values = FormStateData.FormValues;
        if(DataHelper.ContainsPkValues(FormElement,values))
            actionMap.PkFieldValues = DataHelper.GetPkValues(FormElement, values);

        return actionMap;
    }
}