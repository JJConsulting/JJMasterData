using System.Threading.Tasks;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface IActionPlugin<in TPluginAction> where TPluginAction : PluginAction
{
    public string Title { get; set; }

    public Task ExecuteActionAsync(TPluginAction pluginAction);
}