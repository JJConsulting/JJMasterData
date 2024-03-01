#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Structure;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementInfo
{
    public required string Name { get; init; }
    
    public required string TableName { get; init; }
    
    public required string Info { get; init; }
    
    public required bool EnableApi { get; init; }
    
    public required DateTime Modified { get; init; }

    public FormElementInfo()
    {
        
    }
    
    [SetsRequiredMembers]
    [JsonConstructor]
    public FormElementInfo(string name, string tableName, string info, bool enableApi, DateTime modified )
    {
        Name = name;
        TableName = tableName;
        Info = info;
        EnableApi = enableApi;
        Modified = modified;
    }
    
    [SetsRequiredMembers]
    public FormElementInfo(Element formElement, DateTime modified)
    {
        Name = formElement.Name;
        TableName =formElement.TableName;
        Info = formElement.Info;
        EnableApi = formElement.EnableSynchronism;
        Modified = modified;
    }

    public static FormElementInfo FromDictionary(Dictionary<string, object> dictionary)
    {
        return new FormElementInfo
        {
            Info = (string)dictionary[DataDictionaryStructure.Info]!,
            Modified = (DateTime)dictionary[DataDictionaryStructure.LastModified]!,
            Name = (string)dictionary[DataDictionaryStructure.Name]!,
            EnableApi = dictionary[DataDictionaryStructure.EnableSynchronism] as bool? ?? false,
            TableName = (string)dictionary[DataDictionaryStructure.TableName]!
        };
    }
    
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { DataDictionaryStructure.Info, Info },
            { DataDictionaryStructure.Type, "F"},
            { DataDictionaryStructure.LastModified, Modified },
            { DataDictionaryStructure.Name, Name },
            { DataDictionaryStructure.EnableSynchronism, EnableApi },
            { DataDictionaryStructure.TableName,TableName }
        };
    }
}