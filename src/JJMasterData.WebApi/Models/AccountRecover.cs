using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace JJMasterData.WebApi.Models;

[DataContract(Name = "accountRecover")]
public class AccountRecover
{
    /// <summary>
    /// User login
    /// </summary>
    [Required]
    [DataMember(Name = "user")]
    public string? User { get; set; }

    /// <summary>
    /// AppId number from smartphone
    /// </summary>
    [DataMember(Name = "appId")]
    public string? AppId { get; set; }

}