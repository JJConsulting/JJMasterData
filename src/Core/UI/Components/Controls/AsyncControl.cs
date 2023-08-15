#nullable enable

using System;
using System.Threading.Tasks;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Abstractions;

public abstract class AsyncControl : ControlBase
{
    public AsyncControl(IHttpContext currentContext) : base(currentContext)
    {
    }

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
    protected abstract Task<HtmlBuilder> RenderHtmlAsync();
}