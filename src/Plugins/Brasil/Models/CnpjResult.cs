using System;
using System.Collections.Generic;

namespace JJMasterData.Brasil.Models;

using Newtonsoft.Json;

public class CnpjResult
{
    /// <summary>
    /// Razão social.
    /// </summary>
    [JsonProperty("Nome")]
    public required string Nome { get; set; }

    /// <summary>
    /// Nome fantasia.
    /// </summary>
    [JsonProperty("Fantasia")]
    public required string Fantasia { get; set; }

    /// <summary>
    /// Endereço
    /// </summary>
    [JsonProperty("Logradouro")]
    public required string Logradouro { get; set; }

    /// <summary>
    /// Número.
    /// </summary>
    [JsonProperty("Numero")]
    public required string Numero { get; set; }

    /// <summary>
    /// Complemento.
    /// </summary>
    [JsonProperty("Complemento")]
    public required string Complemento { get; set; }

    /// <summary>
    /// CEP sem mascara no formato 00000000.
    /// </summary>
    [JsonProperty("Cep")]
    public required string Cep { get; set; }

    /// <summary>
    /// Nome do Bairro.
    /// </summary>
    [JsonProperty("Bairro")]
    public required string Bairro { get; set; }

    /// <summary>
    /// Nome do Município.
    /// </summary>
    [JsonProperty("Municipio")]
    public required string Municipio { get; set; }

    /// <summary>
    /// Sigla da Unidade da Federação.
    /// </summary>
    [JsonProperty("UF")]
    public required string Uf { get; set; }

    /// <summary>
    /// E-Mail.
    /// </summary>
    [JsonProperty("Email")]
    public required string Email { get; set; }

    /// <summary>
    /// Telefone.
    /// </summary>
    [JsonProperty("Telefone")]
    public required string Telefone { get; set; }

    /// <summary>
    /// Situação.
    /// </summary>
    [JsonProperty("Situacao")]
    public required string Situacao { get; set; }

    /// <summary>
    /// CNAE
    /// </summary>
    [JsonProperty("Atividade_principal")]
    public required CnaeResult AtividadePrincipal { get; set; }

    /// <summary>
    /// Capital Social
    /// </summary>
    [JsonProperty("Capital_social")]
    public required string CapitalSocial { get; set; }

    /// <summary>
    /// Quadro Sócios
    /// </summary>
    [JsonProperty("Quadro_socios")]
    public string[]? QuadroSocios { get; set; }
    
    
    /// <summary>
    /// Data da Abertura
    /// </summary>
    [JsonProperty("Abertura")]
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