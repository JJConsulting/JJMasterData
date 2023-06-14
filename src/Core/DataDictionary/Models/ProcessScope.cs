namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Escopo de execução do pocesso
/// </summary>

public enum ProcessScope
{
    /// <summary>
    /// Global
    /// </summary>
    /// <remarks>
    /// Somente uma importação desse dicionário poderá ser excutada por vez 
    /// e todos os usuários poderão visualizar o último log 
    /// e a execução de um processo em andamento.
    /// </remarks>
    Global = 0,

    /// <summary>
    /// Usuário
    /// </summary>
    /// <remarks>
    /// Varios usuários poderão executar esse processo simultaneamente, 
    /// porém visualiza apenas o log e a execução do seu processo.
    /// </remarks>
    User = 1
}