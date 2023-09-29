using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Components.FormView;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Protheus.Abstractions;

namespace JJMasterData.Protheus;

public class ProtheusPluginActionHandler : IPluginActionHandler
{
    private IProtheusService ProtheusService { get; }
    private ActionScripts ActionScripts { get; }
    private MessageBoxFactory MessageBoxFactory { get; }
    public Guid Id => GuidGenerator.FromValue(nameof(ProtheusPluginActionHandler));
    public string Title => "Protheus";

    public IEnumerable<string> FieldMapKeys => new List<string>();
    public HtmlBuilder? AdditionalInformationHtml => null;

    public ProtheusPluginActionHandler(
        IProtheusService protheusService, 
        HtmlComponentFactory htmlComponentFactory,
        ActionScripts actionScripts
        )
    {
        ProtheusService = protheusService;
        ActionScripts = actionScripts;
        MessageBoxFactory = htmlComponentFactory.MessageBox;
    }
    
    public bool CanCreate(ActionSource actionSource) => actionSource is not ActionSource.Field;

    public async Task<PluginActionResult> ExecuteActionAsync(PluginActionContext context)
    {
        var formElement = context.ActionContext.FormElement;
        var pluginAction =
            formElement.Options.FormToolbarActions.First(a =>
                a is PluginAction pa && pa.PluginId == Id);
        
        await Task.Delay(1);
        
        var messageBox = MessageBoxFactory.Create();
        messageBox.Title = "Selecione uma opção";
        messageBox.Icon = MessageIcon.Info;
        messageBox.Size = MessageSize.Default;
        messageBox.Content = "Pan";
        messageBox.Button1Label = "Rodar novamente";
        messageBox.Button1JsCallback = ActionScripts.GetFormActionScript(pluginAction,context.ActionContext,ActionSource.FormToolbar,false);
        
        return new PluginActionResult
        {
            JsCallback = messageBox.GetShowScript()
        };

    }
}