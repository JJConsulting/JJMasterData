﻿#nullable enable

using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JJMasterData.Core.DataManager;

public static class DataHelper
{
    public static string? GetCurrentUserId(IDictionary<string,dynamic>? userValues)
    {
        if (userValues != null && userValues.TryGetValue("USERID", out var value))
        {
            return value!.ToString();
        }

        var currentContext = JJHttpContext.GetInstance();
        if (currentContext.HasContext() &&
            currentContext.Session?["USERID"] != null)
        {
            return currentContext.Session["USERID"];
        }

        return null;
    }

    /// <summary>
    /// Returns a list with only the primary keys of the table, if the PK value does not exist,
    /// an exception will be thrown
    /// </summary>
    public static Hashtable GetPkValues(Element element, IDictionary<string,dynamic> values)
    {
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var primaryKeys = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        var elementPks = element.Fields.ToList().FindAll(x => x.IsPk);

        if (elementPks == null || elementPks.Count == 0)
            throw new JJMasterDataException($"Primary key not defined for dictionary {element.Name}");

        foreach (var field in elementPks)
        {
            if (!values.ContainsKey(field.Name))
                throw new JJMasterDataException($"Primary key {field.Name} not entered");

            primaryKeys.Add(field.Name, values[field.Name]);
        }

        return primaryKeys;
    }

    public static Dictionary<string,dynamic> GetPkValues(Element element, string parsedValues, char separator)
    {
        var primaryKeys = new Dictionary<string, dynamic>(StringComparer.InvariantCultureIgnoreCase);

        var values = parsedValues.Split(separator);
        if (values == null || values.Length == 0)
            throw new ArgumentException(Translate.Key("Invalid parameter or not found in {values}"), nameof(values));

        var elementPks = element.Fields.ToList().FindAll(x => x.IsPk);
        if (values.Length != elementPks.Count)
            throw new JJMasterDataException(Translate.Key("Invalid primary key"));
            
        for (int i = 0; i < values.Length; i++)
        {
            primaryKeys.Add(elementPks[i].Name, values[i]);
        }

        return primaryKeys;
    }

    /// <summary>
    /// Concat primary keys with separator characters
    /// </summary>
    public static string ParsePkValues(FormElement formElement, IDictionary<string,dynamic>formValues, char separator)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        if (formValues == null)
            throw new ArgumentNullException(nameof(formValues));

        var elementPks = formElement.Fields.ToList().FindAll(x => x.IsPk);
        if (elementPks == null || elementPks.Count == 0)
            throw new JJMasterDataException($"Primary key not defined for dictionary {formElement.Name}");
            
        string name = string.Empty;
        foreach (var field in elementPks)
        {
            if (name.Length > 0)
                name += separator.ToString();
                
            if (!formValues.ContainsKey(field.Name) || formValues[field.Name] == null)
                throw new JJMasterDataException($"Primary key {field.Name} not entered");
                    
            string value = formValues[field.Name]!.ToString()!;
            if (value.Contains(separator))
                throw new JJMasterDataException($"Primary key value {value} contains invalid characters.");
                
            name += value;
        }
            
        return name;
    }

    public static string ParsePkValues(FormElement formElement, DataRow row, char separator)
    {
        var formValues = row.Table.Columns
            .Cast<DataColumn>()
            .ToDictionary(col => col.ColumnName.ToLower(), col => row[col.ColumnName.ToLower()], StringComparer.InvariantCultureIgnoreCase);

        return ParsePkValues(formElement, formValues, separator);
    }

    /// <summary>
    /// Preserves the original name of the field as registered in the dictionary
    /// and validates if the field exists
    /// </summary>
    public static Hashtable? ParseOriginalName(FormElement formElement, Hashtable? paramValues)
    {
        if (paramValues == null)
            return null;

        var filters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (DictionaryEntry entry in paramValues)
        {
            var field = formElement.Fields[entry.Key.ToString()];
            if (!filters.ContainsKey(entry.Key.ToString()!))
                filters.Add(field.Name, entry.Value);
        }

        return filters;
    }

    public static void CopyIntoDictionary(ref IDictionary<string,dynamic>newValues, IDictionary<string,dynamic>? valuesToBeCopied, bool replaceIfExistKey)
    {
        if (valuesToBeCopied == null || valuesToBeCopied.Count == 0)
            return;

        foreach (var entry in valuesToBeCopied)
        {
            if (newValues.ContainsKey(entry.Key))
            {
                if (replaceIfExistKey)
                    newValues[entry.Key] = entry.Value;
            }
            else
            {
                newValues.Add(entry.Key, entry.Value);
            }
        }
    }

}