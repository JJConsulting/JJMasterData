using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class LookupParameters
{
    public string ElementName { get; set; }
    public string ComponentName { get; set; }
    public string FieldKey { get; set; }
    public bool EnableElementActions { get; set; }
    public IDictionary<string, dynamic> Filters { get; set; }

    public LookupParameters(
        string elementName, 
        string componentName, 
        string fieldKey,
        bool enableElementActions,
        IDictionary<string, dynamic> filters)
    {
        ElementName = elementName;
        ComponentName = componentName;
        FieldKey = fieldKey;
        EnableElementActions = enableElementActions;
        Filters = filters;
    }
    
    public LookupParameters()
    {
        
    }
    
    public string ToQueryString(IExpressionsService expressionsService, FormStateData formStateData)
    {
        var queryString = new StringBuilder();

        queryString.Append("elementName=");
        queryString.Append(ElementName);
        queryString.Append("&fieldKey=");
        queryString.Append(FieldKey);
        queryString.Append("&componentName=");
        queryString.Append(ComponentName);
        queryString.Append("&enableAction=");
        queryString.Append(EnableElementActions ? "1" : "0");
        
        if (Filters is { Count: > 0 })
        {
            foreach (var filter in Filters)
            {
                string filterParsed = expressionsService.ParseExpression(filter.Value.ToString(), formStateData, false);
                queryString.Append('&');
                queryString.Append(filter.Key);
                queryString.Append('=');
                queryString.Append(filterParsed);
            }
        }

        return queryString.ToString();
    }
    
    public static LookupParameters FromQueryString(string paramString)
    {
        var parameters = new LookupParameters();
        var keyValuePairs = paramString.Split('&');

        foreach (var pair in keyValuePairs)
        {
            var keyValue = pair.Split('=');
            var key = Uri.UnescapeDataString(keyValue[0]);
            var value = Uri.UnescapeDataString(keyValue[1]);

            switch (key)
            {
                case "elementName":
                    parameters.ElementName = value;
                    break;
                case "fieldKey":
                    parameters.FieldKey = value;
                    break;
                case "componentName":
                    parameters.ComponentName = value;
                    break;
                case "enableAction":
                    parameters.EnableElementActions = value == "1";
                    break;
                default:
                    parameters.Filters ??= new Dictionary<string, object>();
                    parameters.Filters.Add(key, value);
                    break;
            }
        }
        
        parameters.Filters ??= new Dictionary<string, object>();

        return parameters;
    }
}