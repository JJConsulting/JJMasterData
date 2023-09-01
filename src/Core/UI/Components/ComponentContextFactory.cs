using System;
using System.Web;

namespace JJMasterData.Core.UI.Components;

internal static class ComponentContextFactory
{
    public static ComponentContext FromQueryString(string queryString)
    {
        var values = HttpUtility.ParseQueryString(queryString);

        if (Enum.TryParse<ComponentContext>(values["context"], out var context))
        {
            return context;
        }

        return ComponentContext.RenderComponent;
    }
}