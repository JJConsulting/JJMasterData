namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Primeira opção na JJComboBox
/// </summary>
public enum FirstOptionMode
{
    /// <summary>
    /// Somente os registros da lista
    /// </summary>
    None = 1,

    /// <summary>
    /// Incluir como primeira opção valor: (Todos)
    /// </summary>
    All = 2,

    /// <summary>
    /// Incluir como primeira opçãoo valor: (Selecione)
    /// </summary>
    Choose = 3
}