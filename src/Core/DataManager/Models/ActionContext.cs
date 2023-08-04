#nullable enable

using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Models;

public class ActionContext
{
    public required FormElement FormElement { get; init; }
    public required FormStateData FormStateData { get; init; }
    public required string ParentComponentName { get; init; }
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
    
    public ActionMap ToActionMap(string actionName, ActionSource actionSource) => new()
    {
        ActionName = actionName,
        DictionaryName = FormElement.Name,
        ActionSource = actionSource,
        FieldName = FieldName
    };
}