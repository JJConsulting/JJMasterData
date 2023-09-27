namespace JJMasterData.Brasil.Models;

using Newtonsoft.Json;

internal class SintegraCpfDto
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("cpf")]
    public string Cpf { get; set; }

    [JsonProperty("nome")]
    public string Nome { get; set; }

    [JsonProperty("data_nascimento")]
    public string DataNascimento { get; set; }

    [JsonProperty("situacao_cadastral")]
    public string SituacaoCadastral { get; set; }

    [JsonProperty("data_inscricao")]
    public string DataInscricao { get; set; }

    [JsonProperty("genero")]
    public string Genero { get; set; }

    [JsonProperty("uf")]
    public string[] UF { get; set; }

    [JsonProperty("digito_verificador")]
    public string DigitoVerificador { get; set; }

    [JsonProperty("comprovante")]
    public string Comprovante { get; set; }

    [JsonProperty("html")]
    public string Html { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

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
