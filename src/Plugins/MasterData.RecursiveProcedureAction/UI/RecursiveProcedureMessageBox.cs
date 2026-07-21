using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Web.Components;

namespace JJMasterData.RecursiveProcedureAction.UI;

internal class RecursiveProcedureMessageBox(PluginActionContext context)
    : RecursiveProcedureComponent(context)
{
    public PluginActionResult GetResult(RecursiveProcedureOutputParameters outputParameters, int executionSequence)
    {
        var messageBox = new JJMessageBox();

        messageBox.Title = GetTitle();

        if (outputParameters.MessageTypeParameter.Value == null ||
            outputParameters.MessageTypeParameter.Value == DBNull.Value)
            throw new JJMasterDataException($"Variavel {RecursiveProcedureOutputParameters.ParamMessageType} não pode ser nula");

        if (outputParameters.MessageSizeParameter.Value == null ||
            outputParameters.MessageSizeParameter.Value == DBNull.Value)
            throw new JJMasterDataException($"Variavel {RecursiveProcedureOutputParameters.ParamMessageSize} não pode ser nula");

        messageBox.Icon = (MessageIcon)(int)outputParameters.MessageTypeParameter.Value;
        messageBox.Size = (MessageSize)(int)outputParameters.MessageSizeParameter.Value;

        messageBox.Content = outputParameters.MessageContentParameter.Value as string;
        messageBox.Button1Label = "Ok";

        const string hideModalScript = "MessageBox.hide();";

        if (outputParameters.EndExecutionParameter.Value is true)
        {
            if ((int)outputParameters.MessageTypeParameter.Value == (int)MessageIcon.Info)
            {
                messageBox.Button1JsCallback = GetGoBackScript();
            }
            else
            {
                messageBox.Button1JsCallback = hideModalScript + GetExecutionSequenceScript(executionSequence);
            }
        }
        else
        {
            messageBox.Button1JsCallback = GetRecursiveActionScript();
        }

        if ((int)outputParameters.MessageTypeParameter.Value == (int)MessageIcon.Question)
        {
            messageBox.Button1Label = "Sim";
            messageBox.Button2Label = "Não";
            messageBox.Button2JsCallback = hideModalScript + GetExecutionSequenceScript(0);
        }

        return new PluginActionResult
        {
            JsCallback = messageBox.GetShowScript() + GetExecutionSequenceScript(executionSequence)
        };
    }
    
    public PluginActionResult GoBackResult()
    {
        return new PluginActionResult
        {
            JsCallback = GetGoBackScript()
        };
    }

    private string GetGoBackScript()
    {
        var actionContext = Context;
        
        var backAction = actionContext.FormElement.Options.FormToolbarActions.BackAction;

        backAction.IsSubmit = true;

        var backActionContext = actionContext with { Action = backAction };

        return GetExecutionSequenceScript(-1) + GetExecuteActionScript(backActionContext.Id);
    }
}