using System.Text.Json.Serialization;

namespace JJMasterData.Brasil.Models;



internal sealed class SintegraCpfDto
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("cpf")]
    public required string Cpf { get; set; }

    [JsonPropertyName("nome")]
    public required string Nome { get; set; }

    [JsonPropertyName("data_nascimento")]
    public required string DataNascimento { get; set; }

    [JsonPropertyName("situacao_cadastral")]
    public required string SituacaoCadastral { get; set; }

    [JsonPropertyName("data_inscricao")]
    public required string DataInscricao { get; set; }

    [JsonPropertyName("genero")]
    public required string Genero { get; set; }

    [JsonPropertyName("uf")]
    public required string[] UF { get; set; }

    [JsonPropertyName("digito_verificador")]
    public required string DigitoVerificador { get; set; }

    [JsonPropertyName("comprovante")]
    public required string Comprovante { get; set; }

    [JsonPropertyName("html")]
    public required string Html { get; set; }

    [JsonPropertyName("version")]
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
