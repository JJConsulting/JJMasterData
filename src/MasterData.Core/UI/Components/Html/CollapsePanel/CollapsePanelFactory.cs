using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class CollapsePanelFactory(IFormValues formValues) : IComponentFactory<JJCollapsePanel>
{
    public JJCollapsePanel Create()
    {
        return new JJCollapsePanel(formValues);
    }
}