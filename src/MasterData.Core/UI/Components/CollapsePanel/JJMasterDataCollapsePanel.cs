using JJConsulting.Html.Bootstrap.Components;

namespace JJMasterData.Core.UI.Components;

internal sealed class JJMasterDataCollapsePanel : JJCollapsePanel
{
    public JJMasterDataCollapsePanel(IHttpContextAccessor httpContextAccessor)
    {
        var request = httpContextAccessor.HttpContext?.Request;
        if (request?.HasFormContentType == true &&
            request.Form.TryGetValue($"{Name}-is-open", out var value))
        {
            IsOpen = "1".Equals(value.ToString()) ? true : null;
        }
    }
    public JJMasterDataCollapsePanel(IHttpContextAccessor httpContextAccessor, string name)
    {
        Name = name;
        var request = httpContextAccessor.HttpContext?.Request;
        if (request?.HasFormContentType == true &&
            request.Form.TryGetValue($"{Name}-is-open", out var value))
        {
            IsOpen = "1".Equals(value.ToString()) ;
        }
    }
}
