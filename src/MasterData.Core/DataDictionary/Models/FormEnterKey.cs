namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Comportamento da tecla enter no formulário
/// </summary>
public enum FormEnterKey
{
    /// <summary>
    /// Desabilita tecla enter (default)
    /// </summary>
    Disabled = 0,

    /// <summary>
    /// Executa ação padrão 
    /// </summary>
    Submit = 1,

    /// <summary>
    /// Mesmo comportamento da tecla tab
    /// </summary>
    Tab = 2
}