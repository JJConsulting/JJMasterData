using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace JJMasterData.Brasil.Models;

internal sealed class CpfCnpjCnpjDto
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("erro")]
    public string? Erro { get; set; }

    [JsonPropertyName("erroCodigo")]
    public int? ErroCodigo { get; set; }

    [JsonPropertyName("razao")]
    public string? Razao { get; set; }

    [JsonPropertyName("fantasia")]
    public string? Fantasia { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("capitalSocial")]
    public decimal? CapitalSocial { get; set; }

    [JsonPropertyName("inicioAtividade")]
    public string? InicioAtividade { get; set; }

    [JsonPropertyName("matrizEndereco")]
    public CpfCnpjEnderecoDto? MatrizEndereco { get; set; }

    [JsonPropertyName("situacao")]
    public CpfCnpjSituacaoDto? Situacao { get; set; }

    [JsonPropertyName("cnae")]
    public CpfCnpjCnaeDto? Cnae { get; set; }

    [JsonPropertyName("telefones")]
    public CpfCnpjTelefoneDto[]? Telefones { get; set; }

    [JsonPropertyName("socios")]
    public CpfCnpjSocioDto[]? Socios { get; set; }

    internal CnpjResult ToCnpjResult()
    {
        var endereco = MatrizEndereco;

        return new CnpjResult
        {
            Nome = Razao ?? string.Empty,
            Fantasia = Fantasia ?? string.Empty,
            Logradouro = endereco?.Logradouro ?? string.Empty,
            Numero = endereco?.Numero ?? string.Empty,
            Complemento = endereco?.Complemento ?? string.Empty,
            Cep = OnlyDigits(endereco?.Cep),
            Bairro = endereco?.Bairro ?? string.Empty,
            Municipio = endereco?.Cidade ?? string.Empty,
            Uf = endereco?.Uf ?? string.Empty,
            Email = Email ?? string.Empty,
            Telefone = FormatTelefone(),
            Situacao = Situacao?.Nome ?? string.Empty,
            AtividadePrincipal = new CnaeResult
            {
                Code = Cnae?.Fiscal ?? string.Empty,
                Text = Cnae?.Descricao ?? string.Empty
            },
            CapitalSocial = CapitalSocial?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            QuadroSocios = Socios?
                .Where(socio => !string.IsNullOrWhiteSpace(socio.Nome))
                .Select(socio => socio.Nome!)
                .ToArray(),
            Abertura = ParseDate(InicioAtividade)
        };
    }

    private string FormatTelefone()
    {
        var telefone = Telefones?.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Numero));

        if (telefone is null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(telefone.Ddd)
            ? telefone.Numero ?? string.Empty
            : $"({telefone.Ddd}) {telefone.Numero}";
    }

    private static string OnlyDigits(string? value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : new string(value.Where(char.IsDigit).ToArray());
    }

    private static DateTime ParseDate(string? value)
    {
        return DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var date)
            ? date
            : DateTime.MinValue;
    }
}

internal sealed class CpfCnpjEnderecoDto
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("numero")]
    public string? Numero { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("cidade")]
    public string? Cidade { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }
}

internal sealed class CpfCnpjSituacaoDto
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }
}

internal sealed class CpfCnpjCnaeDto
{
    [JsonPropertyName("fiscal")]
    public string? Fiscal { get; set; }

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }
}

internal sealed class CpfCnpjTelefoneDto
{
    [JsonPropertyName("ddd")]
    public string? Ddd { get; set; }

    [JsonPropertyName("numero")]
    public string? Numero { get; set; }
}

internal sealed class CpfCnpjSocioDto
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }
}
