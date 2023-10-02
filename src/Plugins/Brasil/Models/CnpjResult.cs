namespace JJMasterData.Brasil.Models;


using Newtonsoft.Json;

public class CnpjResult
{
    /// <summary>
    /// Razão social.
    /// </summary>
    [JsonProperty("Nome")]
    public string Nome { get; set; }

    /// <summary>
    /// Nome fantasia.
    /// </summary>
    [JsonProperty("Fantasia")]
    public string Fantasia { get; set; }

    /// <summary>
    /// Endereço
    /// </summary>
    [JsonProperty("Logradouro")]
    public string Logradouro { get; set; }

    /// <summary>
    /// Número.
    /// </summary>
    [JsonProperty("Numero")]
    public string Numero { get; set; }

    /// <summary>
    /// Complemento.
    /// </summary>
    [JsonProperty("Complemento")]
    public string Complemento { get; set; }

    /// <summary>
    /// CEP sem mascara no formato 00000000.
    /// </summary>
    [JsonProperty("Cep")]
    public string Cep { get; set; }

    /// <summary>
    /// Nome do Bairro.
    /// </summary>
    [JsonProperty("Bairro")]
    public string Bairro { get; set; }

    /// <summary>
    /// Nome do Município.
    /// </summary>
    [JsonProperty("Municipio")]
    public string Municipio { get; set; }

    /// <summary>
    /// Sigla da Unidade da Federação.
    /// </summary>
    [JsonProperty("UF")]
    public string UF { get; set; }

    /// <summary>
    /// E-Mail.
    /// </summary>
    [JsonProperty("Email")]
    public string Email { get; set; }

    /// <summary>
    /// Telefone.
    /// </summary>
    [JsonProperty("Telefone")]
    public string Telefone { get; set; }

    /// <summary>
    /// Situação.
    /// </summary>
    [JsonProperty("Situacao")]
    public string Situacao { get; set; }

    /// <summary>
    /// CNAE
    /// </summary>
    [JsonProperty("Atividade_principal")]
    public CnaeResult AtividadePrincipal { get; set; }

    /// <summary>
    /// Capital Social
    /// </summary>
    [JsonProperty("Capital_social")]
    public string CapitalSocial { get; set; }

    /// <summary>
    /// Quadro Sócios
    /// </summary>
    [JsonProperty("Quadro_socios")]
    public string[] QuadroSocios { get; set; }

    /// <summary>
    /// Data da Abertura
    /// </summary>
    [JsonProperty("Abertura")]
    public string Abertura { get; set; }
}
