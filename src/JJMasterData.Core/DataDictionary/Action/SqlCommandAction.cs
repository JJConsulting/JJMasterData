using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class SqlCommandAction : BasicAction
{
    /// <summary>
    /// Comando SQL a ser executado, aceita expression
    /// </summary>
    [DataMember(Name = "commandSQL")]
    public string CommandSql { get; set; }

    /// <summary>
    /// Aplicar somenter as linhas selecionadas (default=false)
    /// </summary>
    /// <remarks>
    /// Valido somente para contexto da toolbar, 
    /// onde o comando é aplicado para cada linha selecionada
    /// </remarks>
    [DataMember(Name = "applyOnSelected")]
    public bool ApplyOnSelected { get; set; }

    public override bool IsUserCreated => true;
    
    public SqlCommandAction()
    {
        Icon = IconType.Play;
    }
}