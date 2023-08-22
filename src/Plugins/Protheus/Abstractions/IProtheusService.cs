namespace JJMasterData.Protheus.Abstractions;

/// <summary>
/// Classe responsável por gerenciar as conexões e chamadas para o Protheus
/// </summary>
public interface IProtheusService
{
    Task<string> CallFunctionAsync(string urlProtheus, string functionName, string parms);
}