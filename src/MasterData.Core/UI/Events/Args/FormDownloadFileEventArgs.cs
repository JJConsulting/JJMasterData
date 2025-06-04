using System;
using System.IO;

namespace JJMasterData.Core.UI.Events.Args;

public class FormDownloadFileEventArgs(string fileName, MemoryStream file) : EventArgs
{
    /// <summary>
    /// Nome do arquivo
    /// </summary>
    public string FileName { get; set; } = fileName;

    /// <summary>
    /// Conteúdo do arquivo
    /// </summary>
    public MemoryStream File { get; set; } = file;

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string ErrorMessage { get; set; }
}
