using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager;

public class ActionMap
{
    [JsonProperty("actionName")]
    public string ActionName { get; set; }

    [JsonProperty("fieldName")]
    public string FieldName { get; set; }

    [JsonProperty("pkFieldValues")]
    public IDictionary<string,dynamic>PkFieldValues { get; set; }

    [JsonProperty("contextAction")]
    public ActionSource ActionSource { get; set; }

    public ActionMap()
    {
        
    }
    
    public ActionMap(ActionSource actionSource)
    {
        ActionSource = actionSource;
        PkFieldValues = new Dictionary<string, dynamic>();
    }

    public ActionMap(ActionSource actionSource, FormElement formElement, IDictionary<string,dynamic> row, string actionName)
    {
        ActionSource = actionSource;
        ActionName = actionName;
        PkFieldValues = new Dictionary<string, dynamic>();
        foreach (var f in formElement.Fields.ToList().FindAll(x => x.IsPk).Where(f => row.ContainsKey(f.Name) && row[f.Name] != null))
        {
            PkFieldValues.Add(f.Name, row[f.Name].ToString());
        }
    }

    [Obsolete("Please use EncryptionService.EncryptActionMap(ActionMap). DES has known vulnerabilities.")]
    public string GetCriptJson()
    {
        string jsonMap = JsonConvert.SerializeObject(this);
        string criptMap = Cript.Cript64(jsonMap);
        return criptMap;
    }

}
