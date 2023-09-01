#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.UI.Components;

internal class RouteContext
{
    public required string CurrentElementName { get; set; }
    public string? ParentElementName { get; set; }
    public required ComponentContext ComponentContext { get; set; }

    [SetsRequiredMembers]
    private RouteContext(string currentElementName, string? parentElementName, ComponentContext componentContext = ComponentContext.RenderComponent)
    {
        ComponentContext = componentContext;
        CurrentElementName = currentElementName;
        ParentElementName = parentElementName;
    }
    
    [SetsRequiredMembers]
    public RouteContext(FormElement formElement, ComponentContext componentContext)
    {
        ComponentContext = componentContext;
        CurrentElementName = formElement.Name;
        ParentElementName = formElement.ParentName;
    }
    
    public static RouteContext FromQueryString(string queryString)
    {
        var parsedQuery = HttpUtility.ParseQueryString(queryString);
        
        var currentElementName = parsedQuery["currentElementName"]!;
        var parentElementName = parsedQuery["parentElementName"];
        var componentContextString = parsedQuery["componentContext"];

        if (Enum.TryParse<ComponentContext>(componentContextString, out var componentContext))
        {
            return new RouteContext(currentElementName, parentElementName, componentContext);
        }
        
        return new RouteContext(currentElementName, parentElementName, componentContext);
    }
}