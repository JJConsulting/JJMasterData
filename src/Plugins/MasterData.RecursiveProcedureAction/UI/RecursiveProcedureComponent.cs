using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Web.Components;

namespace JJMasterData.RecursiveProcedureAction.UI;

internal abstract class RecursiveProcedureComponent(PluginActionContext context)
{
    protected PluginActionContext Context { get; } = context;
    
    protected string GetTitle()
    {
        if (Context.ConfigurationMap.TryGetValue(RecursiveProcedurePluginActionHandler.ConfigTitle, out var title) && !string.IsNullOrEmpty(title?.ToString()))
        {
            return title.ToString()!;
        }

        return "Sem Título";
    }
    
    protected string GetRecursiveActionScript()
    {
        return GetExecuteActionScript(Context.Id);
    }
    
    protected static string GetExecuteActionScript(string actionId)
    {
        return $"ActionHelper.executeAction('{actionId}')";
    }
    
    protected static string GetExecutionSequenceScript(int executionSequence)
    {
        return $"setExecutionSequence({executionSequence});";
    }
}