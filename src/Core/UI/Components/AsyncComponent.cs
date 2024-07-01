#nullable enable

using System.Threading.Tasks;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// A ComponentBase with asynchronous programming support.
/// </summary>
public abstract class AsyncComponent : ComponentBase
{
    public async Task<ComponentResult> GetResultAsync()
    {
        if (Visible)
            return await BuildResultAsync();

        return EmptyComponentResult.Value;
    }

    protected abstract Task<ComponentResult> BuildResultAsync();
}