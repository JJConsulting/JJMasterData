using System;

namespace JJMasterData.Core.FormEvents.Args;

public class FormDeleteFileEventArgs : EventArgs
{
    /// <summary>
    /// Nome do arquivo
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string? ErrorMessage { get; set; }

    public FormDeleteFileEventArgs(string fileName)
    {
        FileName = fileName;
    }
}
