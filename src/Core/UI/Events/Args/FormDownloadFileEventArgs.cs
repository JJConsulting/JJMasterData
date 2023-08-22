using System;
using System.IO;

namespace JJMasterData.Core.FormEvents.Args;

public class FormDownloadFileEventArgs : EventArgs
{
    /// <summary>
    /// Nome do arquivo
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Conteúdo do arquivo
    /// </summary>
    public MemoryStream File { get; set; }

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string? ErrorMessage { get; set; }

    public FormDownloadFileEventArgs(string fileName, MemoryStream file)
    {
        FileName = fileName;
        File = file;
    }
}
