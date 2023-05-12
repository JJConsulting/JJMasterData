using System;
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormElementInfo
{
    public string Name { get; set; }
    
    public string TableName { get; set; }
    
    public string Info { get; set; }
    
    public string Sync { get; set; }
    
    public DateTime Modified { get; set; }
    
    [JsonConstructor]
    public FormElementInfo(string name, string tableName, string info, string sync, DateTime modified )
    {
        Name = name;
        TableName = tableName;
        Info = info;
        Sync = sync;
        Modified = modified;
    }
    

    public FormElementInfo(Element formElement, DateTime modified)
    {
        Name = formElement.Name;
        TableName =formElement.TableName;
        Info = formElement.Info;
        Sync = formElement.Sync ? "1" : "0";
        Modified = modified;
    }
}