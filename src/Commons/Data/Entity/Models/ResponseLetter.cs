using System;
using System.Collections;
using System.Collections.Generic;
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
    public IDictionary<string,dynamic> ValidationList { get; set; }

    [JsonProperty("data")]
    public IDictionary<string,dynamic>  Data { get; set; }

    public ResponseLetter() { }

    public ResponseLetter(string message)
    {
        Message = message;
    }
        
}