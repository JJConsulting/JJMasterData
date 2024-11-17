using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public sealed class FormToolbarActionList : FormElementActionList
{
    [JsonPropertyName("saveAction")]
    public SaveAction SaveAction { get; set; } = new();
    
    [JsonPropertyName("backAction")]
    public BackAction BackAction { get; set; } = new();
    
    [JsonPropertyName("cancelAction")]
    public CancelAction CancelAction { get; set; } = new();
    
    [JsonPropertyName("formEditAction")]
    public FormEditAction FormEditAction { get; set; } = new();
    
    [JsonPropertyName("auditLogFormToolbarAction")]
    public AuditLogFormToolbarAction AuditLogFormToolbarAction { get; set; } = new();

    public FormToolbarActionList DeepCopy()
    {
        return new FormToolbarActionList
        {
            SaveAction = (SaveAction)SaveAction.DeepCopy(),
            BackAction = (BackAction)BackAction.DeepCopy(),
            CancelAction = (CancelAction)CancelAction.DeepCopy(),
            FormEditAction = (FormEditAction)FormEditAction.DeepCopy(),
            AuditLogFormToolbarAction = (AuditLogFormToolbarAction)AuditLogFormToolbarAction.DeepCopy(),
            SqlActions = SqlActions.ConvertAll(action => (SqlCommandAction)action.DeepCopy()),
            UrlActions = UrlActions.ConvertAll(action => (UrlRedirectAction)action.DeepCopy()),
            HtmlTemplateActions = HtmlTemplateActions.ConvertAll(action => (HtmlTemplateAction)action.DeepCopy()),
            JsActions = JsActions.ConvertAll(action => (ScriptAction)action.DeepCopy()),
            PluginActions = PluginActions.ConvertAll(action => (PluginAction)action.DeepCopy())        };
    }

    protected override IEnumerable<BasicAction> GetActions()
    {
        yield return SaveAction;
        yield return BackAction;
        yield return CancelAction;
        yield return FormEditAction;
        yield return AuditLogFormToolbarAction;
    }
}
