using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class NavFactory : IComponentFactory<JJTabNav>
{
    private readonly IHttpContext _httpContext;

    public NavFactory(IHttpContext httpContext)
    {
        _httpContext = httpContext;
    }
    
    public JJTabNav Create()
    {
        return new JJTabNav(_httpContext);
    }
}