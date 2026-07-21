using System.Text;
using System.Web;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using JJConsulting.Html.Extensions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Web.Utils;
using JJMasterData.Web.Components;

namespace JJMasterData.RecursiveProcedureAction.UI;

internal class RecursiveProcedureObsModal(
    PluginActionContext context,
    IComponentFactory componentFactory)
    : RecursiveProcedureComponent(context)
{
    private IComponentFactory ComponentFactory { get; } = componentFactory;

    public PluginActionResult GetResult()
    {
        var modalDialog = new JJModalDialog
        {
            Name = $"executionSequenceObsModal-{Context.Action.Name}",
            Title = GetTitle(),
            Size = GetModalSize(),
            ShowAsOpened = false,
            Attributes =
            {
                ["data-bs-backdrop"] = "static",
                ["data-bs-keyboard"] = "false"
            }
        };
        SetModalBody(modalDialog);
        SetModalButtons(modalDialog);
        
        var modalDialogHtml = modalDialog.GetHtml();

        var jsCallback = new StringBuilder();
        jsCallback.Append(
            $"getMasterDataForm().insertAdjacentHTML('beforeend','{HttpUtility.JavaScriptStringEncode(modalDialogHtml)}');");
        jsCallback.Append($"bootstrap.Modal.getOrCreateInstance('#{modalDialog.Name}').show();");
        jsCallback.Append($"listenAllEvents('#{modalDialog.Name}');");

        return new PluginActionResult
        {
            JsCallback = jsCallback.ToString()
        };
    }

    private ModalSize GetModalSize()
    {
        if (!Context.ConfigurationMap.TryGetValue(RecursiveProcedurePluginActionHandler.ConfigObsModalSize,
                out var modalSizeText))
        {
            return ModalSize.Large;
        }

        if (Enum.TryParse<ModalSize>(modalSizeText?.ToString(), out var modalSize))
        {
            return modalSize;
        }

        return ModalSize.Large;
    }
    
    private void SetModalBody(JJModalDialog modalDialog)
    {
        var label = HtmlBuilder.Label();
        label.WithCssClass(BootstrapHelper.Label);
        label.AppendText(Context.ConfigurationMap[RecursiveProcedurePluginActionHandler.ConfigObsLabel]?.ToString() ?? "Observação");
        label.WithAttribute("for", "ExecutionSequenceObs");
        
        var observationHtml = new HtmlBuilder();
        observationHtml.Append(label);

        var textArea = ComponentFactory.Controls.TextArea.Create();
        textArea.Name = "ExecutionSequenceObs";
        textArea.Enabled = true;
        textArea.MaxLength = -1;

        observationHtml.Append(textArea.GetHtmlBuilder());

        modalDialog.Content = observationHtml;
    }

    private void SetModalButtons(JJModalDialog modalDialog)
    {
        var button1 = new JJLinkButton
        {
            Text = "Ok",
            CssClass = "btn btn-primary",
            ShowAsButton = true
        };

        var hideDialogScript = $"hideDialogs('{modalDialog.Name}');";
        
        button1.OnClientClick = hideDialogScript + GetExecutionSequenceScript(0) + GetRecursiveActionScript();

        var button2 = new JJLinkButton
        {
            ShowAsButton = true,
            Text = "Cancelar",
            OnClientClick = hideDialogScript
        };

        modalDialog.Buttons.Add(button1);
        modalDialog.Buttons.Add(button2);
 
     
    }
}