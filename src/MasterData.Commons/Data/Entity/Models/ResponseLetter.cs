using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace JJMasterData.Commons.Data.Entity.Models;

public class ResponseLetter
{
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("validationList")]
    public Dictionary<string, string> ValidationList { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, object>  Data { get; set; }

    public ResponseLetter() { }

    public ResponseLetter(string message)
    {
        Message = message;
    }
        
}