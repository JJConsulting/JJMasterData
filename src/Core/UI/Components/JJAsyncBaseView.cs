#nullable enable

using System.Threading.Tasks;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// A JJBaseView with asynchronous programming support.
/// </summary>
public abstract class JJAsyncBaseView : JJBaseView
{
    public async Task<string?> GetHtmlAsync()
    {
        var htmlBuilder = await RenderHtmlAsync();
        return Visible ? htmlBuilder?.ToString() : null;
    }

    public async Task<HtmlBuilder?> GetHtmlBuilderAsync()
    {
        return Visible ? await RenderHtmlAsync() : null;
    }
    
    internal abstract override HtmlBuilder RenderHtml();
    protected abstract Task<HtmlBuilder?> RenderHtmlAsync();
}