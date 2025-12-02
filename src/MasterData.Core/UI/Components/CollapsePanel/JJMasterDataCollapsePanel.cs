using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.CollapsePanel;

internal sealed class JJMasterDataCollapsePanel : JJCollapsePanel
{
    public JJMasterDataCollapsePanel(IFormValues formValues)
    {
        var collapseMode = formValues[$"{Name}-is-open"];
        IsOpen = string.IsNullOrEmpty(collapseMode) ? ExpandedByDefault : "1".Equals(collapseMode);
    }
}