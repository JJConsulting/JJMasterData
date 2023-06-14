using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;

[JsonObject("accountChangePassword")]
public class AccountChange
{
    /// <summary>
    /// User login
    /// </summary>
    [Required]
    [JsonProperty("user")]
    public string? User { get; set; }

    /// <summary>
    /// Current Password
    /// </summary>
    [Required]
    [JsonProperty("pwdCurrent")]
    public string? PwdCurrent { get; set; }

    /// <summary>
    /// New Password
    /// </summary>
    [Required]
    [JsonProperty("pwdNew")]
    public string? PwdNew { get; set; }

    /// <summary>
    /// Confirm Password
    /// </summary>
    [Required]
    [JsonProperty("pwdConfirm")]
    public string? PwdConfirm { get; set; }
}