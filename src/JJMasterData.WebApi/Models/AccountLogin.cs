using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;

[JsonObject("accountLogin")]
public class AccountLogin
{
    /// <summary>
    /// User login
    /// </summary>
    [Required]
    [JsonProperty("user")]
    public string? User { get; set; }

    /// <summary>
    /// Current password without cryptography
    /// </summary>
    [Required]
    [JsonProperty("password")]
    public string? Password { get; set; }

    /// <summary>
    /// AppId number from smartphone,  
    /// it's used on recover password
    /// </summary>
    [JsonProperty("appId")]
    public string? AppId { get; set; }

}