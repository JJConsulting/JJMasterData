#nullable enable

namespace JJMasterData.Core.UI.Components;

public class RouteContext
{
    public string? ElementName { get; set; }
    public string? ParentElementName { get; set; }
    public ComponentContext ComponentContext { get; set; }

    internal RouteContext()
    {
        ComponentContext = ComponentContext.RenderComponent;
    }

    internal RouteContext(string? elementName, string? parentElementName, ComponentContext componentContext)
    {
        ComponentContext = componentContext;
        ElementName = elementName;
        ParentElementName = parentElementName;
    }
    
    public bool CanRender(string elementName)
    {
        if (ParentElementName is not null)
        {
            return ParentElementName == elementName || ParentElementName == elementName;
        }

        return IsCurrentFormElement(elementName);
    }
    
    public bool IsCurrentFormElement(string elementName)
    {
        if (ElementName is null) 
            return true;
        
        if (elementName.Equals(ElementName))
            return true;

        return false;
    }
}