using System;
using System.Collections;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity;

public class ResponseLetter
{
    [JsonProperty("status")]
    public int? Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("validationList")]
    public Hashtable ValidationList { get; set; }

    [JsonProperty("data")]
    public Hashtable Data { get; set; }

    public ResponseLetter() { }

    public ResponseLetter(string message)
    {
        Message = message;
    }
        
}