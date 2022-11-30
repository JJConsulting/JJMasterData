using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace JJMasterData.Api.Models;

[DataContract(Name = "accountLogin")]
public class AccountLogin
{
    /// <summary>
    /// User login
    /// </summary>
    [Required]
    [DataMember(Name = "user")]
    public string? User { get; set; }

    /// <summary>
    /// Current password without cryptography
    /// </summary>
    [Required]
    [DataMember(Name = "password")]
    public string? Password { get; set; }

    /// <summary>
    /// AppId number from smartphone,  
    /// it's used on recover password
    /// </summary>
    [DataMember(Name = "appId")]
    public string? AppId { get; set; }

}