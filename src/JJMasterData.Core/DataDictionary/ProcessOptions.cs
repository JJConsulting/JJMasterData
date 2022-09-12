using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

public class ProcessOptions
{
    /// <summary>
    /// Comando SQL a ser executado antes de iniciar o processo de importação
    /// </summary>
    [DataMember(Name = "commandBeforeProcess")]
    public string CommandBeforeProcess { get; set; }

    /// <summary>
    /// Comando SQL a ser executado no final do processo de importação
    /// </summary>
    [DataMember(Name = "commandAfterProcess")]
    public string CommandAfterProcess { get; set; }

    /// <summary>
    /// Escopo de execução do pocesso
    /// </summary>
    /// <remarks>
    /// Global<para></para>
    /// Somente uma importação desse dicionário poderá ser excutada por vez 
    /// e todos os usuários poderão visualizar o último log 
    /// e a execução de um processo em andamento.
    /// <para></para>
    /// User<para></para>
    /// Varios usuários poderão executar esse processo simultaneamente, 
    /// porém visualiza apenas o log e a execução do seu processo.
    /// </remarks>
    [DataMember(Name = "scope")]
    public ProcessScope Scope { get; set; }

    public ProcessOptions()
    {
        Scope = ProcessScope.Global;
    }
}