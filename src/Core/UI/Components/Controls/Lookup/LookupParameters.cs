using System;
using System.Collections.Generic;
using System.Text;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components;

public class LookupParameters
{
    public string ElementName { get; set; }
    public string ComponentName { get; set; }
    public string FieldKeyName { get; set; }
    public string FieldValueName { get; set; }
    public bool EnableElementActions { get; set; }
    public IDictionary<string, object> Filters { get; set; }

    public LookupParameters(
        string elementName, 
        string componentName, 
        string fieldKeyName,
        string fieldValueName,
        bool enableElementActions,
        IDictionary<string, object> filters)
    {
        ElementName = elementName;
        ComponentName = componentName;
        FieldKeyName = fieldKeyName;
        FieldValueName = fieldValueName;
        EnableElementActions = enableElementActions;
        Filters = filters;
    }
    
    public LookupParameters()
    {
        
    }
    
    public string ToQueryString()
    {
        var queryString = new StringBuilder();

        queryString.Append("elementName=");
        queryString.Append(ElementName);
        queryString.Append("&fieldKeyName=");
        queryString.Append(FieldKeyName);
        queryString.Append("&fieldValueName=");
        queryString.Append(FieldValueName);
        queryString.Append("&componentName=");
        queryString.Append(ComponentName);
        queryString.Append("&enableAction=");
        queryString.Append(EnableElementActions ? "1" : "0");

        if (Filters is not { Count: > 0 })
            return queryString.ToString();
        
        foreach (var filter in Filters)
        {
            queryString.Append('&');
            queryString.Append(filter.Key);
            queryString.Append('=');
            queryString.Append(filter.Value);
        }

        return queryString.ToString();
    }
    
    public string ToQueryString(ExpressionsService expressionsService, FormStateData formStateData)
    {
        var queryString = new StringBuilder();

        queryString.Append("elementName=");
        queryString.Append(ElementName);
        queryString.Append("&fieldKeyName=");
        queryString.Append(FieldKeyName);
        queryString.Append("&fieldValueName=");
        queryString.Append(FieldValueName);
        queryString.Append("&componentName=");
        queryString.Append(ComponentName);
        queryString.Append("&enableAction=");
        queryString.Append(EnableElementActions ? "1" : "0");
        
        if (Filters is { Count: > 0 })
        {
            foreach (var filter in Filters)
            {
                string filterParsed = expressionsService.ReplaceExpressionWithParsedValues(filter.Value.ToString(), formStateData);
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
                case "fieldKeyName":
                    parameters.FieldKeyName = value;
                    break;
                case "fieldValueName":
                    parameters.FieldValueName = value;
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