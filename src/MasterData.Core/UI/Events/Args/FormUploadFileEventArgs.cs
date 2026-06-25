#nullable disable warnings
using System;

namespace JJMasterData.Core.UI.Events.Args;

public class FormUploadFileEventArgs(IFormFile file) : EventArgs
{
    /// <summary>
    /// Arquivo recebido
    /// </summary>
    public IFormFile File { get; set; } = file;

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Mensagem referente a validação do evento (opcional)
    /// </summary>
    public string SuccessMessage { get; set; }
}
