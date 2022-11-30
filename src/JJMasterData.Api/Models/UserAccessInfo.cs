using System.Runtime.Serialization;

namespace JJMasterData.Api.Models;

/// <summary>
/// Informações do login do usuário
/// </summary>
[DataContract(Name = "userAccessInfo")]
public class UserAccessInfo
{
    /// <summary>
    /// Código do erro:
    /// <br/>0   = OK
    /// <br/>100 = Unexpected error
    /// <br/>101 = Password sent by email if existing
    /// <br/>110 = Invalid Access
    /// <br/>112 = Sua conta excedeu o limite de tentitivas
    /// <br/>113 = Blocked user
    /// <br/>130 = Password change required
    /// <br/>131 = Your password will expire soon, change it as soon as possible
    /// <br/>301 = Password cannot be blank!
    /// <br/>302 = Password cannot contain user!
    /// <br/>303 = Password must contain at least one number!
    /// <br/>304 = The password must contain at least one uppercase character!
    /// <br/>305 = The password contains at least one of the following characters: !@#$%^*()_+<![CDATA[&]]> 
    /// <br/>306 = Password cannot contain a colon(:)
    /// <br/>307 = Password and confirmation password are not the same!
    /// <br/>308 = Your current password is incorrect! It will not be possible to change to the new password.
    /// </summary>
    [DataMember(Name = "errorId")]
    public int ErrorId { get; set; }

    /// <summary>
    /// Retorna verdadeiro se o login foi realizado com sucesso
    /// </summary>
    [DataMember(Name = "isValid")]
    public bool IsValid { get; set; }

    /// <summary>
    /// Descrição do erro
    /// </summary>
    [DataMember(Name = "message")]
    public string? Message { get; set; }

    /// <summary>
    /// Token a ser utilizado nas requisições
    /// </summary>
    [DataMember(Name = "token")]
    public string? Token { get; set; }

    /// <summary>
    /// Id do Usuário
    /// </summary>
    [DataMember(Name = "userId")]
    public string? UserId { get; set; }

    /// <summary>
    /// Versão do sistema
    /// </summary>
    [DataMember(Name = "version")]
    public string? Version { get; set; }
}