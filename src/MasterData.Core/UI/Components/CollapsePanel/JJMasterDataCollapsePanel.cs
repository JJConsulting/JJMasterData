using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal sealed class JJMasterDataCollapsePanel : JJCollapsePanel
{
    public JJMasterDataCollapsePanel(IFormValues formValues)
    {
        if (formValues.TryGetValue($"{Name}-is-open", out var value))
        {
            IsOpen = "1".Equals(value);
        }
    }
}