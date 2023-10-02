using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity.Models;

public class ResponseLetter
{
    [JsonProperty("status")]
    public int? Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("validationList")]
    public IDictionary<string, string> ValidationList { get; set; }

    [JsonProperty("data")]
    public IDictionary<string, object>  Data { get; set; }

    public ResponseLetter() { }

    public ResponseLetter(string message)
    {
        Message = message;
    }
        
}