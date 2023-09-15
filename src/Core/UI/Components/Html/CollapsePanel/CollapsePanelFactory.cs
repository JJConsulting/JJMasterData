using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class CollapsePanelFactory : IComponentFactory<JJCollapsePanel>
{
    private readonly IHttpContext _currentContext;

    public CollapsePanelFactory(IHttpContext currentContext)
    {
        _currentContext = currentContext;
    }
    
    public JJCollapsePanel Create()
    {
        return new JJCollapsePanel(_currentContext);
    }
}