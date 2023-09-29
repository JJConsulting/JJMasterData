#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface IPluginFieldActionHandler : IPluginHandler
{
    public IEnumerable<string> FieldMapKeys { get; }
    public Task<PluginActionResult> ExecuteActionAsync(PluginFieldActionContext context);
}