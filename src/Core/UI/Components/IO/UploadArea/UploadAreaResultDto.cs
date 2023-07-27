using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public record UploadAreaResultDto
{
    [JsonProperty("jquery-upload-file-message", NullValueHandling=NullValueHandling.Ignore)]
    public string Message { get; set; }
        
    [JsonProperty("jquery-upload-file-error", NullValueHandling=NullValueHandling.Ignore)]
    public string Error { get; set; }
    public string ToJson() => JsonConvert.SerializeObject(this);
}