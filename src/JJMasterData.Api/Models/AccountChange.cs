using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace JJMasterData.Api.Models;

[DataContract(Name = "accountChangePassword")]
public class AccountChange
{
    /// <summary>
    /// User login
    /// </summary>
    [Required]
    [DataMember(Name = "user")]
    public string User { get; set; }

    /// <summary>
    /// Current Password
    /// </summary>
    [Required]
    [DataMember(Name = "pwdCurrent")]
    public string PwdCurrent { get; set; }

    /// <summary>
    /// New Password
    /// </summary>
    [Required]
    [DataMember(Name = "pwdNew")]
    public string PwdNew { get; set; }

    /// <summary>
    /// Confirm Password
    /// </summary>
    [Required]
    [DataMember(Name = "pwdConfirm")]
    public string PwdConfirm { get; set; }
}