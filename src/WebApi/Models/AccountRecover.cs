using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;

[JsonObject("accountRecover")]
public class AccountRecover
{
    /// <summary>
    /// User login
    /// </summary>
    [Required]
    [JsonProperty("user")]
    public string? User { get; set; }

    /// <summary>
    /// AppId number from smartphone
    /// </summary>
    [JsonProperty("appId")]
    public string? AppId { get; set; }

}