using System;

namespace JJMasterData.Core.FormEvents.Args;

/// <summary>
/// Argumento utilizado para customizar o conteúdo renderizado em uma celula
/// </summary>
public class ExportContentEventArgs : EventArgs
{
    /// <summary>
    /// Retorno esperado com o conteudo HTML renderizado
    /// </summary>
    public string Value { get; set; }
}
