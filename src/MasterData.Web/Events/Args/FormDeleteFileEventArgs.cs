#nullable disable warnings
using System;

namespace JJMasterData.Web.Events.Args;

public class FormDeleteFileEventArgs(string fileName) : EventArgs
{
    /// <summary>
    /// Nome do arquivo
    /// </summary>
    public string FileName { get; set; } = fileName;

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string ErrorMessage { get; set; }
}
