using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager;

[DataContract]
internal class ActionMap
{
    [DataMember(Name = "actionName")]
    public string ActionName { get; set; }

    [DataMember(Name = "fieldName")]
    public string FieldName { get; set; }

    [DataMember(Name = "pkFieldValues")]
    public Hashtable PKFieldValues { get; set; }

    [DataMember(Name = "contextAction")]
    public ActionSource ContextAction { get; set; }

    public ActionMap()
    {
        
    }
    
    public ActionMap(ActionSource contextAction)
    {
        ContextAction = contextAction;
        PKFieldValues = new Hashtable();
    }

    public ActionMap(ActionSource contextAction, FormElement formElement, Hashtable row, string actionName)
    {
        ContextAction = contextAction;
        ActionName = actionName;
        PKFieldValues = new Hashtable();
        foreach (var f in formElement.Fields.ToList().FindAll(x => x.IsPk))
        {
            if (row[f.Name] != null)
                PKFieldValues.Add(f.Name, row[f.Name].ToString());
        }
    }

    public string GetCriptJson()
    {
        string jsonMap = JsonConvert.SerializeObject(this);
        string criptMap = Cript.Cript64(jsonMap);
        return criptMap;
    }

}
