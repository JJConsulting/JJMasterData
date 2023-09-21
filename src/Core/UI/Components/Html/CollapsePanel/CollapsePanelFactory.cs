using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Components;

public class CollapsePanelFactory : IComponentFactory<JJCollapsePanel>
{
    private readonly IFormValues _formValues;

    public CollapsePanelFactory(IFormValues formValues)
    {
        _formValues = formValues;
    }
    
    public JJCollapsePanel Create()
    {
        return new JJCollapsePanel(_formValues);
    }
}