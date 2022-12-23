using JJMasterData.Commons.Dao;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents.Factories;

public class CollapsePanelFactory
{
    private readonly IHttpContext _httpContext;

    public CollapsePanelFactory(IHttpContext httpContext)
    {
        _httpContext = httpContext;
    }

    public JJCollapsePanel CreateCollapsePanel()
    {
        return new JJCollapsePanel(_httpContext);
    }
}