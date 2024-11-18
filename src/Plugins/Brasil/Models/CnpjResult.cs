using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Brasil.Models;



public class CnpjResult
{
    /// <summary>
    /// Razão social.
    /// </summary>
    [JsonPropertyName("Nome")]
    public required string Nome { get; set; }

    /// <summary>
    /// Nome fantasia.
    /// </summary>
    [JsonPropertyName("Fantasia")]
    public required string Fantasia { get; set; }

    /// <summary>
    /// Endereço
    /// </summary>
    [JsonPropertyName("Logradouro")]
    public required string Logradouro { get; set; }

    /// <summary>
    /// Número.
    /// </summary>
    [JsonPropertyName("Numero")]
    public required string Numero { get; set; }

    /// <summary>
    /// Complemento.
    /// </summary>
    [JsonPropertyName("Complemento")]
    public required string Complemento { get; set; }

    /// <summary>
    /// CEP sem mascara no formato 00000000.
    /// </summary>
    [JsonPropertyName("Cep")]
    public required string Cep { get; set; }

    /// <summary>
    /// Nome do Bairro.
    /// </summary>
    [JsonPropertyName("Bairro")]
    public required string Bairro { get; set; }

    /// <summary>
    /// Nome do Município.
    /// </summary>
    [JsonPropertyName("Municipio")]
    public required string Municipio { get; set; }

    /// <summary>
    /// Sigla da Unidade da Federação.
    /// </summary>
    [JsonPropertyName("UF")]
    public required string Uf { get; set; }

    /// <summary>
    /// E-Mail.
    /// </summary>
    [JsonPropertyName("Email")]
    public required string Email { get; set; }

    /// <summary>
    /// Telefone.
    /// </summary>
    [JsonPropertyName("Telefone")]
    public required string Telefone { get; set; }

    /// <summary>
    /// Situação.
    /// </summary>
    [JsonPropertyName("Situacao")]
    public required string Situacao { get; set; }

    /// <summary>
    /// CNAE
    /// </summary>
    [JsonPropertyName("Atividade_principal")]
    public required CnaeResult AtividadePrincipal { get; set; }

    /// <summary>
    /// Capital Social
    /// </summary>
    [JsonPropertyName("Capital_social")]
    public required string CapitalSocial { get; set; }

    /// <summary>
    /// Quadro Sócios
    /// </summary>
    [JsonPropertyName("Quadro_socios")]
    public string[]? QuadroSocios { get; set; }
    
    
    /// <summary>
    /// Data da Abertura
    /// </summary>
    [JsonPropertyName("Abertura")]
    public required DateTime Abertura { get; set; }
    
    public Dictionary<string, object?> ToDictionary()
    {
        var dictionary = new Dictionary<string, object?>
        {
            { nameof(Nome), Nome },
            { nameof(Fantasia), Fantasia },
            { nameof(Logradouro), Logradouro },
            { nameof(Numero), Numero },
            { nameof(Complemento), Complemento },
            { nameof(Cep), Cep },
            { nameof(Bairro), Bairro },
            { nameof(Municipio), Municipio },
            { nameof(Uf), Uf },
            { nameof(Email), Email },
            { nameof(Telefone), Telefone },
            { nameof(Situacao), Situacao },
            { "AtividadePrincipal.Codigo", AtividadePrincipal.Code },
            { "AtividadePrincipal.Descricao", AtividadePrincipal.Text },
            { nameof(CapitalSocial), CapitalSocial },
            { nameof(QuadroSocios), QuadroSocios != null ? string.Join(", ", QuadroSocios) : "" },
            { nameof(Abertura), Abertura }
        };

        return dictionary;
    }

}