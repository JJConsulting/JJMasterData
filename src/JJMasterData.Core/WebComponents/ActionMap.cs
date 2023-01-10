using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.Core.WebComponents;

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
    public ActionOrigin ContextAction { get; set; }
    
    public ActionMap()
    {
        
    }
    
    public ActionMap(ActionOrigin contextAction)
    {
        ContextAction = contextAction;
        PKFieldValues = new Hashtable();
    }

    public ActionMap(ActionOrigin contextAction, FormElement formElement, Hashtable row, string actionName)
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

    public string GetEncryptedJson(JJMasterDataEncryptionService service)
    {
        string jsonMap = JsonConvert.SerializeObject(this);
        string criptMap = service.EncryptString(jsonMap);
        return criptMap;
    }

}
