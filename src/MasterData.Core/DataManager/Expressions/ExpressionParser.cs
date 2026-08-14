using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Expressions;

public sealed class ExpressionParser(
    IHttpContextAccessor httpContextAccessor,
    IMasterDataUser masterDataUser,
    ILogger<ExpressionParser> logger)
{
    public Dictionary<string, object?> ParseExpression(
        string? expression,
        FormStateData formStateData)
    {
        if (string.IsNullOrEmpty(expression))
            return new(StringComparer.InvariantCultureIgnoreCase);

        var fields = StringManager.FindValuesByInterval(
            expression,
            ExpressionHelper.Begin,
            ExpressionHelper.End).ToHashSet();

        var result = new Dictionary<string, object?>(
            fields.Count,
            StringComparer.InvariantCultureIgnoreCase);

        foreach (var field in fields)
        {
            var value = GetParsedValue(field, formStateData);
            result[field] = value;
            logger.LogExpressionParsedValue(field, value);
        }

        return result;
    }

    private object? GetParsedValue(string field, FormStateData formStateData)
    {
        var pageState = formStateData.PageState;

        switch (field.ToLowerInvariant())
        {
            case "pagestate":
                return pageState.GetPageStateName();
            case "islist":
                return pageState is PageState.List ? 1 : 0;
            case "isview":
                return pageState is PageState.View ? 1 : 0;
            case "isupdate":
                return pageState is PageState.Update ? 1 : 0;
            case "isinsert":
                return pageState is PageState.Insert ? 1 : 0;
            case "isfilter":
                return pageState is PageState.Filter ? 1 : 0;
            case "isimport":
                return pageState is PageState.Import ? 1 : 0;
            case "isdelete":
                return pageState is PageState.Delete ? 1 : 0;
            case "fieldname":
                return httpContextAccessor.HttpContext?.Request.Query["fieldName"].ToString();
            case "userid":
                return masterDataUser.Id;
            case "currentculture":
                return CultureInfo.CurrentCulture.Name;
            case "useremail":
                return GetClaimValue(ClaimTypes.Email);
            case "legacyid":
                return GetClaimValue("LegacyId");
        }

        if (formStateData.UserValues != null && formStateData.UserValues.TryGetValue(field, out var value))
            return value;

        if (formStateData.Values.TryGetValue(field, out var objValue))
        {
            if (objValue is bool boolValue)
                return boolValue ? "1" : "0";
            
            if (objValue is string stringValue && !string.IsNullOrEmpty(stringValue))
                return stringValue;
            
            if (objValue is not null)
                return objValue;
        }

        var session = httpContextAccessor.HttpContext?.Features.Get<ISessionFeature>()?.Session;

        if (session != null && session.TryGetValue(field, out var sessionValue))
            return Encoding.UTF8.GetString(sessionValue);
       
        return GetClaimValue(field);
    }

    private string? GetClaimValue(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }
}
