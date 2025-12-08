using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal sealed class JJMasterDataTabNav : JJTabNav
{
    public JJMasterDataTabNav(IFormValues formValues)
    {
        if (formValues.TryGetValue("selected_tab_" + Name, out var value))
        {
            SelectedTabIndex = int.TryParse(value, out var intIndex) ? intIndex : 0;
        }
        else
        {
            SelectedTabIndex = 0;
        }
    }
}