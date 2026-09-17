namespace JJMasterData.Brasil.Configuration;

public class CpfCnpjSettings
{
    /// <summary>
    /// Base URL of the cpfcnpj.com.br API. The token, package and document are appended to the path.
    /// </summary>
    public string Url { get; set; } = "api.cpfcnpj.com.br/";

    /// <summary>
    /// Access token used on the request path.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Package used for CNPJ lookups. Package 6 returns registration status, size and Simples Nacional data.
    /// </summary>
    public int CnpjPackage { get; set; } = 6;

    /// <summary>
    /// Package used for CPF lookups.
    /// </summary>
    public int CpfPackage { get; set; } = 2;
}
