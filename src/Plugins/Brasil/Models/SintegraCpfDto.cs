namespace JJMasterData.Brasil.Models;

using Newtonsoft.Json;

internal sealed class SintegraCpfDto
{
    [JsonProperty("code")]
    public required string Code { get; set; }

    [JsonProperty("status")]
    public required string Status { get; set; }

    [JsonProperty("message")]
    public required string Message { get; set; }

    [JsonProperty("cpf")]
    public required string Cpf { get; set; }

    [JsonProperty("nome")]
    public required string Nome { get; set; }

    [JsonProperty("data_nascimento")]
    public required string DataNascimento { get; set; }

    [JsonProperty("situacao_cadastral")]
    public required string SituacaoCadastral { get; set; }

    [JsonProperty("data_inscricao")]
    public required string DataInscricao { get; set; }

    [JsonProperty("genero")]
    public required string Genero { get; set; }

    [JsonProperty("uf")]
    public required string[] UF { get; set; }

    [JsonProperty("digito_verificador")]
    public required string DigitoVerificador { get; set; }

    [JsonProperty("comprovante")]
    public required string Comprovante { get; set; }

    [JsonProperty("html")]
    public required string Html { get; set; }

    [JsonProperty("version")]
    public required string Version { get; set; }

    public CpfResult ToCpfResult()
    {
        return new CpfResult
        {
            SituacaoCadastral = SituacaoCadastral,
            ComprovanteEmitido = Comprovante,
            NomeDaPf = Nome
        };
    }
}
