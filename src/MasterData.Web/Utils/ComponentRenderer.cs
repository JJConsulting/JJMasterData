using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace JJMasterData.Web.Utils;

public sealed class ComponentRenderer(HtmlRenderer htmlRenderer)
{
    public async Task<string> RenderAsync<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var html = await htmlRenderer.Dispatcher.InvokeAsync(async () =>
        {
            var parameterView = ParameterView.FromDictionary(parameters);
            var output = await htmlRenderer.RenderComponentAsync<T>(parameterView);

            return output.ToHtmlString();
        });
        
        return html;
    }
}