using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class FormElementFieldActionList : FormElementActionList
{
    internal List<PluginFieldAction> PluginFieldActions { get; init; } = [];
    
    public FormElementFieldActionList DeepCopy()
    {
        return new FormElementFieldActionList
        {
            PluginFieldActions = PluginFieldActions.ConvertAll(action => (PluginFieldAction)action.DeepCopy()),
            SqlActions = SqlActions.ConvertAll(action => (SqlCommandAction)action.DeepCopy()),
            UrlActions = UrlActions.ConvertAll(action => (UrlRedirectAction)action.DeepCopy()),
            HtmlTemplateActions = HtmlTemplateActions.ConvertAll(action => (HtmlTemplateAction)action.DeepCopy()),
            JsActions = JsActions.ConvertAll(action => (ScriptAction)action.DeepCopy()),
            PluginActions = PluginActions.ConvertAll(action => (PluginAction)action.DeepCopy())
        };
    }

    protected override IEnumerable<BasicAction> GetActions()
    {
        return PluginFieldActions;
    }
}