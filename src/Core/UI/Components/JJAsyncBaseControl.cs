#nullable enable

using System.Threading.Tasks;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Abstractions;

public abstract class JJAsyncBaseControl : JJBaseControl
{
    public JJAsyncBaseControl(IHttpContext currentContext) : base(currentContext)
    {
    }

    public async Task<string?> GetHtmlAsync()
    {
        return Visible ? (await RenderHtmlAsync()).ToString() : null;
    }

    public async Task<HtmlBuilder?> GetHtmlBuilderAsync()
    {
        return Visible ? await RenderHtmlAsync() : null;
    }
    
    internal abstract override HtmlBuilder RenderHtml();
    protected abstract Task<HtmlBuilder> RenderHtmlAsync();
}