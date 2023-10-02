using System;

namespace JJMasterData.Core.UI.Events.Args;

public class FormRenameFileEventArgs : EventArgs
{
    /// <summary>
    /// Nome do arquivo atual
    /// </summary>
    public string CurrentName { get; set; }

    /// <summary>
    /// Novo Nome do arquivo
    /// </summary>
    public string NewName { get; set; }

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string ErrorMessage { get; set; }

    public FormRenameFileEventArgs(string currentName, string newName)
    {
        CurrentName = currentName;
        NewName = newName;
    }
}
