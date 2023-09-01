using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

public class RouteContextResolver
{
    private RouteContext RouteContext { get; }
    private IQueryString QueryString { get; }

    public RouteContextResolver(RouteContext routeContext, IQueryString queryString)
    {
        RouteContext = routeContext;
        QueryString = queryString;
    }

    public bool CanRender(FormElement formElement)
    {
        if (RouteContext.ParentElementName is not null)
        {
            return RouteContext.ParentElementName == formElement.ParentName || RouteContext.ParentElementName == formElement.Name;
        }

        if (RouteContext.ElementName is not null)
        {
            return RouteContext.ElementName == formElement.Name;
        }
        
        return true;
    }
    
    public bool IsSearchBoxRoute(FormElement formElement, out FormElementField field)
    {
        var fieldName = QueryString["fieldName"];
        if(formElement.Fields.Contains(fieldName))
        {
            field = formElement.Fields[fieldName];
            return true;
        }

        field = null;
        
        return false;
    }
}