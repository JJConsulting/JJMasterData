#nullable enable

using System.Threading.Tasks;
using JJConsulting.Html.Bootstrap.Abstractions;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// A ComponentBase with asynchronous programming support.
/// </summary>
public abstract class AsyncComponent : ComponentBase
{
    public Task<ComponentResult> GetResultAsync()
    {
        if (Visible)
            return BuildResultAsync();

        return Task.FromResult<ComponentResult>(EmptyComponentResult.Value);
    }

    protected abstract Task<ComponentResult> BuildResultAsync();
}