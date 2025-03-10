#nullable enable


using System.Text.Json.Serialization;

namespace JJMasterData.Core.UI.Components;

public class UploadAreaResultDto
{
    [JsonPropertyName("message")]
    public string? SuccessMessage { get; set; }

    [JsonPropertyName("error")]
    public string? ErrorMessage { get; set; }
}