using System;
using System.Collections;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Dao.Entity;

[Serializable]
[DataContract]
public class ResponseLetter
{
    [DataMember(Name = "status")]
    public int? Status { get; set; }

    [DataMember(Name = "message")]
    public string Message { get; set; }

    [DataMember(Name = "validationList")]
    public Hashtable ValidationList { get; set; }

    [DataMember(Name = "data")]
    public Hashtable Data { get; set; }

    public ResponseLetter() { }

    public ResponseLetter(string message)
    {
        Message = message;
    }
        
}