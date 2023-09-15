#nullable enable

using JJMasterData.Core.DataDictionary;

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

    internal static RouteContext FromFormElement(FormElement formElement,ComponentContext context)
    {
        return new RouteContext(formElement.Name, formElement.ParentName, context);
    }
    
    public bool CanRender(string elementName)
    {
        if (ElementName is null)
            return true;
        
        if (ParentElementName is not null)
            return ParentElementName == elementName || ParentElementName == elementName;

        return IsCurrentFormElement(elementName);
    }
    
    public bool IsCurrentFormElement(string elementName)
    {
        return ElementName is null || elementName.Equals(ElementName);
    }
}