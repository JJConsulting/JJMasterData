using System;
using System.Diagnostics.CodeAnalysis;
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormElementInfo
{
    public required string Name { get; init; }
    
    public required string TableName { get; init; }
    
    public required string Info { get; init; }
    
    public required string Sync { get; init; }
    
    public required DateTime Modified { get; init; }

    public FormElementInfo()
    {
        
    }
    
    [SetsRequiredMembers]
    [JsonConstructor]
    public FormElementInfo(string name, string tableName, string info, string sync, DateTime modified )
    {
        Name = name;
        TableName = tableName;
        Info = info;
        Sync = sync;
        Modified = modified;
    }
    
    [SetsRequiredMembers]
    public FormElementInfo(Element formElement, DateTime modified)
    {
        Name = formElement.Name;
        TableName =formElement.TableName;
        Info = formElement.Info;
        Sync = formElement.EnableWebApi ? "1" : "0";
        Modified = modified;
    }
}