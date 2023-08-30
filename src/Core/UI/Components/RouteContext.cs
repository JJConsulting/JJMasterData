#nullable enable

namespace JJMasterData.Core.UI.Components;

internal class RouteContext
{
    public string CurrentElementName { get; set; }
    public string? ParentElementName { get; set; }
    public ComponentContext ComponentContext { get; set; }

    public RouteContext(ComponentContext componentContext, string currentElementName, string? parentElementName)
    {
        ComponentContext = componentContext;
        CurrentElementName = currentElementName;
        ParentElementName = parentElementName;
    }
}