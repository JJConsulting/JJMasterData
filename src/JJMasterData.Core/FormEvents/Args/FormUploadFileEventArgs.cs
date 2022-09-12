using System;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.FormEvents.Args;

public class FormUploadFileEventArgs : EventArgs
{
    /// <summary>
    /// Arquivo recebido
    /// </summary>
    public JJFormFile File { get; set; }

    /// <summary>
    /// Mensagem de erro referente a validação do evento (opcional)
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Mensagem referente a validação do evento (opcional)
    /// </summary>
    public string SuccessMessage { get; set; }

    public FormUploadFileEventArgs(JJFormFile file)
    {
        File = file;
    }
}
