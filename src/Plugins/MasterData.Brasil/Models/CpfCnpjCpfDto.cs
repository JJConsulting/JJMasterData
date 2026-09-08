using System.Text.Json.Serialization;

namespace JJMasterData.Brasil.Models;

internal sealed class CpfCnpjCpfDto
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("erro")]
    public string? Erro { get; set; }

    [JsonPropertyName("erroCodigo")]
    public int? ErroCodigo { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("situacao")]
    public string? Situacao { get; set; }

    [JsonPropertyName("consultaID")]
    public string? ConsultaId { get; set; }

    internal CpfResult ToCpfResult()
    {
        return new CpfResult
        {
            NomeDaPf = Nome ?? string.Empty,
            SituacaoCadastral = Situacao ?? string.Empty,
            // cpfcnpj.com.br does not return a "comprovante" number; consultaID is the receipt id of
            // the lookup and is the closest analogue for the contract's ComprovanteEmitido field.
            ComprovanteEmitido = ConsultaId ?? string.Empty
        };
    }
}
