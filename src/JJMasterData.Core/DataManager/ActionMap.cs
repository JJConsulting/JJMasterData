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
    [JsonProperty("actionName")]
    public string ActionName { get; set; }

    [JsonProperty("fieldName")]
    public string FieldName { get; set; }

    [JsonProperty("pkFieldValues")]
    public IDictionary PKFieldValues { get; set; }

    [JsonProperty("contextAction")]
    public ActionSource ContextAction { get; set; }

    public ActionMap()
    {
        
    }
    
    public ActionMap(ActionSource contextAction)
    {
        ContextAction = contextAction;
        PKFieldValues = new Hashtable();
    }

    public ActionMap(ActionSource contextAction, FormElement formElement, IDictionary row, string actionName)
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
