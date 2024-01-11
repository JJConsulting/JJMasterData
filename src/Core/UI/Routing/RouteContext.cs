#nullable enable

using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.UI.Routing;

public class RouteContext
{
    public string? ElementName { get; set; }
    public string? ParentElementName { get; set; }
    public ComponentContext ComponentContext { get; set; }
    
    public RouteContext()
    {
        ComponentContext = ComponentContext.RenderComponent;
    }
    
    public RouteContext(ComponentContext componentContext)
    {
        ComponentContext = componentContext;
    }

    internal RouteContext(string? elementName, string? parentElementName, ComponentContext componentContext)
    {
        ComponentContext = componentContext;
        ElementName = elementName;
        ParentElementName = parentElementName;
    }

    public static RouteContext FromFormElement(FormElement formElement,ComponentContext context)
    {
        return new RouteContext(formElement.Name, formElement.ParentName, context);
    }
    
    public bool CanRender(FormElement formElement)
    {
        string currentElementName = formElement.Name;
        string? currentParentName = formElement.ParentName;

        if (ElementName == currentParentName)
            return true;
        
        if (ElementName is null)
            return true;
        
        if (ParentElementName is not null)
            return ParentElementName == currentElementName || ParentElementName == currentParentName;

        return IsCurrentFormElement(currentElementName);
    }
    
    public bool IsCurrentFormElement(string elementName)
    {
        return ElementName is null || elementName.Equals(ElementName);
    }
}
