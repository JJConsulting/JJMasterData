using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class JJMasterDataTabNav : JJTabNav
{
    public JJMasterDataTabNav(IFormValues formValues)
    {
        var tabIndex = formValues["selected_tab_" + Name];

        SelectedTabIndex = int.TryParse(tabIndex, out var intIndex) ? intIndex : 0;
    }
}