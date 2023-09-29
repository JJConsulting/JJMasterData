using JJMasterData.Commons.Security.Hashing;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Protheus;

public class ProtheusPluginActionHandler : IPluginActionHandler
{
    public Guid Id => GuidGenerator.FromValue(nameof(ProtheusPluginActionHandler));
    public string Title => "Protheus";

    public IEnumerable<string> AdditionalParametersHints
    {
        get
        {
            yield return "Url";
        }
    }
    public HtmlBuilder? AdditionalInformationHtml => null;

    public bool CanCreate(ActionSource actionSource) => actionSource is not ActionSource.Field;

    public async Task<PluginActionResult> ExecuteActionAsync(PluginActionContext pluginActionContext)
    {
        var pluginAction =
            pluginActionContext.FormElement.Options.FormToolbarActions.First(a =>
                a is PluginAction pa && pa.PluginId == Id);

        var gridAction =
            pluginActionContext.FormElement.Options.FormToolbarActions.First(a => a is BackAction);
        
        await Task.Delay(1);
        return new PluginActionResult
        {
            Modal =
            new PluginActionModal {
                Title = "Integração Protheus",
                Content = "Selecione uma opção",
                Button1Action = pluginAction,
                Button1Label = "Rodar de novo",
                Button2Label = "Vai pra grid",
                Button2Action = gridAction
            }
        };
    }
}