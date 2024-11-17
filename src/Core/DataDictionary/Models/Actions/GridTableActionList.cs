using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class GridTableActionList : FormElementActionList
{
    [JsonPropertyName("deleteAction")]
    public DeleteAction DeleteAction { get; set; } = new();
    
    [JsonPropertyName("editAction")]
    public EditAction EditAction { get; set; } = new();
    
    [JsonPropertyName("viewAction")]
    public ViewAction ViewAction { get; set; } = new();

    
    public GridTableActionList DeepCopy()
    {
        return new GridTableActionList
        {
            DeleteAction = (DeleteAction)DeleteAction.DeepCopy(),
            EditAction = (EditAction)EditAction.DeepCopy(),
            ViewAction = (ViewAction)ViewAction.DeepCopy(),
            SqlActions = SqlActions.ConvertAll(action => (SqlCommandAction)action.DeepCopy()),
            UrlActions = UrlActions.ConvertAll(action => (UrlRedirectAction)action.DeepCopy()),
            HtmlTemplateActions = HtmlTemplateActions.ConvertAll(action => (HtmlTemplateAction)action.DeepCopy()),
            JsActions = JsActions.ConvertAll(action => (ScriptAction)action.DeepCopy()),
            PluginActions = PluginActions.ConvertAll(action => (PluginAction)action.DeepCopy())
        };
    }

    protected override IEnumerable<BasicAction> GetActions()
    {
        yield return DeleteAction;
        yield return EditAction;
        yield return ViewAction;
    }
}