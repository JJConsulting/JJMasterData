using System;
using System.Linq;
using System.Text.Json.Serialization;


namespace JJMasterData.Brasil.Models;

internal sealed class SintegraCnpjDto
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }

    [JsonPropertyName("cnpj")]
    public required string Cnpj { get; set; }

    [JsonPropertyName("nome")]
    public required string Nome { get; set; }

    [JsonPropertyName("fantasia")]
    public required string Fantasia { get; set; }

    [JsonPropertyName("cep")]
    public required string Cep { get; set; }

    [JsonPropertyName("uf")]
    public required string Uf { get; set; }

    [JsonPropertyName("municipio")]
    public required string Municipio { get; set; }

    [JsonPropertyName("bairro")]
    public required string Bairro { get; set; }

    [JsonPropertyName("tipo_logradouro")]
    public required object TipoLogradouro { get; set; }

    [JsonPropertyName("logradouro")]
    public required string Logradouro { get; set; }

    [JsonPropertyName("numero")]
    public required string Numero { get; set; }

    [JsonPropertyName("complemento")]
    public required string Complemento { get; set; }

    [JsonPropertyName("telefone")]
    public required string Telefone { get; set; }

    [JsonPropertyName("email")]
    public required string Email { get; set; }

    [JsonPropertyName("capital_social")]
    public required string CapitalSocial { get; set; }

    [JsonPropertyName("data_situacao")]
    public required string DataSituacao { get; set; }

    [JsonPropertyName("data_situacao_especial")]
    public required string DataSituacaoEspecial { get; set; }

    [JsonPropertyName("abertura")]
    public required DateTime Abertura { get; set; }

    [JsonPropertyName("motivo_situacao")]
    public required string MotivoSituacao { get; set; }

    [JsonPropertyName("sigla_natureza_juridica")]
    public required string SiglaNaturezaJuridica { get; set; }

    [JsonPropertyName("natureza_juridica")]
    public required string NaturezaJuridica { get; set; }

    [JsonPropertyName("situacao")]
    public required string Situacao { get; set; }

    [JsonPropertyName("situacao_especial")]
    public required string SituacaoEspecial { get; set; }

    [JsonPropertyName("tipo")]
    public required string Tipo { get; set; }

    [JsonPropertyName("atividade_principal")]
    public required AtividadePrincipal[] AtividadePrincipal { get; set; }

    [JsonPropertyName("atividades_secundarias")]
    public required AtividadesSecundaria[] AtividadesSecundarias { get; set; }

    [JsonPropertyName("qsa")]
    public required Qsa[] Qsa { get; set; }

    [JsonPropertyName("ultima_atualizacao")]
    public required string UltimaAtualizacao { get; set; }

    [JsonPropertyName("efr")]
    public required string Efr { get; set; }

    [JsonPropertyName("extra")]
    public object? Extra { get; set; }

    [JsonPropertyName("porte")]
    public required string Porte { get; set; }

    [JsonPropertyName("ibge")]
    public required Ibge Ibge { get; set; }

    [JsonPropertyName("cnpjs_do_grupo")]
    public required CnpjDoGrupo[] CnpjsDoGrupo { get; set; }

    [JsonPropertyName("inscricao_municipal")]
    public required string InscricaoMunicipal { get; set; }

    [JsonPropertyName("coordinates")]
    public required Coordinates Coordinates { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }
    
    internal CnpjResult ToCnpjResult()
    {
        var obj = new CnpjResult
        {
            Nome = Nome,
            Fantasia = Fantasia,
            Logradouro = Logradouro,
            Numero = Nome,
            Complemento = Complemento,
            Cep = Cep,
            Bairro = Bairro,
            Municipio = Municipio,
            Uf = Uf,
            Email = Email,
            Telefone = Telefone,
            Situacao = Situacao,
            CapitalSocial = CapitalSocial,
            Abertura = Abertura,
            AtividadePrincipal = new CnaeResult
            {
                Text = AtividadePrincipal[0].Text,
                Code = AtividadePrincipal[0].Code
            }
        };

        if (AtividadePrincipal.Length > 0)
        {
            var cnae = new CnaeResult
            {
                Text = AtividadePrincipal[0].Text,
                Code = AtividadePrincipal[0].Code
            };
            obj.AtividadePrincipal = cnae;
        }

        if (Qsa is { Length: > 0 })
        {
            obj.QuadroSocios = Qsa.Select(item => item.Nome).ToArray();
        }

        return obj;
    }

}

public class Ibge
{
    [JsonPropertyName("codigo_municipio")]
    public required string CodigoMunicipio { get; set; }

    [JsonPropertyName("codigo_uf")]
    public required string CodigoUf { get; set; }
}

public class Coordinates
{
    [JsonPropertyName("latitude")]
    public required string Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public required string Longitude { get; set; }
}

public class AtividadePrincipal
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class AtividadesSecundaria
{
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

public class Qsa
{
    [JsonPropertyName("qual")]
    public required string Qual { get; set; }

    [JsonPropertyName("qual_rep_legal")]
    public required string QualRepLegal { get; set; }

    [JsonPropertyName("nome_rep_legal")]
    public required string NomeRepLegal { get; set; }

    [JsonPropertyName("pais_origem")]
    public required string PaisOrigem { get; set; }

    [JsonPropertyName("nome")]
    public required string Nome { get; set; }

    [JsonPropertyName("faixa_etaria")]
    public required object FaixaEtaria { get; set; }
}

public class CnpjDoGrupo
{
    [JsonPropertyName("cnpj")]
    public required string Cnpj { get; set; }

    [JsonPropertyName("uf")]
    public required string Uf { get; set; }

    [JsonPropertyName("tipo")]
    public required string Tipo { get; set; }
}