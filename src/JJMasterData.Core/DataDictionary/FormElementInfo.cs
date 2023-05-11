using System;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary;

public class FormElementInfo
{
    public string Name { get; set; }
    
    public string TableName { get; set; }
    
    public string Info { get; set; }
    
    public string Sync { get; set; }
    
    public DateTime Modified { get; set; }
    

    public FormElementInfo(Element formElement, DateTime modified)
    {
        Name = formElement.Name;
        TableName =formElement.TableName;
        Info = formElement.Info;
        Sync = formElement.Sync ? "1" : "0";
        Modified = modified;
    }
}