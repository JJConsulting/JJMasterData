#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Logging;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Expressions;

public sealed class ExpressionParser(
    IHttpContext httpContext,
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
            expression!,
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
        var (values, userValues, pageState) = formStateData;

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
                return httpContext.Request.QueryString["fieldName"];
            case "userid":
                return masterDataUser.Id;
            case "currentculture":
                return CultureInfo.CurrentCulture.Name;
            case "useremail":
                return GetClaimValue(ClaimTypes.Email);
            case "legacyid":
                return GetClaimValue("LegacyId");
        }

        object? parsedValue;

        if (userValues != null && userValues.TryGetValue(field, out var value))
        {
            parsedValue = value;
        }
        else if (values.TryGetValue(field, out var objValue) && !string.IsNullOrEmpty(objValue?.ToString()))
        {
            if (objValue is bool boolValue)
                parsedValue = boolValue ? "1" : "0";
            else
                parsedValue = objValue;
        }
        else if (httpContext.Session.HasSession() && httpContext.Session.HasKey(field))
        {
            parsedValue = httpContext.Session[field];
        }
        else
        {
            parsedValue = GetClaimValue(field) ?? string.Empty;
        }

        return parsedValue;
    }

    private string? GetClaimValue(string claimType)
    {
        return httpContext.User?.FindFirst(claimType)?.Value;
    }
}
