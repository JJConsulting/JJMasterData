#nullable enable

using System;
using System.Threading.Tasks;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// A JJComponentBase with asynchronous programming support.
/// </summary>
public abstract class AsyncComponent : ComponentBase
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
    
    [Obsolete("Please use RenderHtmlAsync")]
    internal override HtmlBuilder RenderHtml()
    {
        return RenderHtmlAsync().GetAwaiter().GetResult() ?? new HtmlBuilder();
    }

    protected abstract Task<HtmlBuilder?> RenderHtmlAsync();
}