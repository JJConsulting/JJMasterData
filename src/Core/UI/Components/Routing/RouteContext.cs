#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.UI.Components;

public class RouteContext
{
    public string? ElementName { get; set; }
    public string? ParentElementName { get; set; }
    public ComponentContext ComponentContext { get; set; }

    public RouteContext()
    {
        ComponentContext = ComponentContext.RenderComponent;
    }
    

    public RouteContext(string? elementName, string? parentElementName, ComponentContext componentContext = ComponentContext.RenderComponent)
    {
        ComponentContext = componentContext;
        ElementName = elementName;
        ParentElementName = parentElementName;
    }
    
    public RouteContext(FormElement formElement, ComponentContext componentContext)
    {
        ComponentContext = componentContext;
        ElementName = formElement.Name;
        ParentElementName = formElement.ParentName;
    }
    
    public bool CanRender(FormElement formElement)
    {
        if (ParentElementName is not null)
        {
            return ParentElementName == formElement.ParentName || ParentElementName == formElement.Name;
        }

        if (ElementName is not null)
        {
            return ElementName == formElement.Name;
        }
        
        return true;
    }
}