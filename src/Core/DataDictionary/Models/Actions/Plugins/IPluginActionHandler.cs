#nullable enable

using System.Threading.Tasks;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface IPluginActionHandler : IPluginHandler
{
    public bool CanCreate(ActionSource actionSource);
    public Task<PluginActionResult> ExecuteActionAsync(PluginActionContext context);
}
