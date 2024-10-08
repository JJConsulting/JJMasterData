#nullable enable
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public record UploadAreaResultDto
{
    [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
    public string? SuccessMessage { get; set; }

    [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
    public string? ErrorMessage { get; set; }
}