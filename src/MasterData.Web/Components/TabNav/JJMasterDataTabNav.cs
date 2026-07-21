using JJConsulting.Html.Bootstrap.Components;

namespace JJMasterData.Web.Components;

internal sealed class JJMasterDataTabNav : JJTabNav
{
    public JJMasterDataTabNav(IHttpContextAccessor formValues)
    {
        var request = formValues.HttpContext?.Request;
        if (request?.HasFormContentType == true &&
            request.Form.TryGetValue("selected_tab_" + Name, out var value))
        {
            SelectedTabIndex = int.TryParse(value.ToString(), out var intIndex) ? intIndex : 0;
        }
        else
        {
            SelectedTabIndex = 0;
        }
    }
}
