#nullable disable warnings
using System;

namespace JJMasterData.Web.Events.Args;

public class FormRenameFileEventArgs(string currentName, string newName) : EventArgs
{
    /// <summary>
    /// Nome do arquivo atual
    /// </summary>
    public string CurrentName { get; set; } = currentName;

    /// <summary>
    /// Novo Nome do arquivo
    /// </summary>
    public string NewName { get; set; } = newName;

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string ErrorMessage { get; set; }
}
