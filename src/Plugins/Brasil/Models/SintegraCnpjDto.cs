using System;
using System.Linq;
using Newtonsoft.Json;

namespace JJMasterData.Brasil.Models;

internal class SintegraCnpjDto
{
    [JsonProperty("code")]
    public required string Code { get; set; }

    [JsonProperty("status")]
    public required string Status { get; set; }

    [JsonProperty("message")]
    public required string Message { get; set; }

    [JsonProperty("cnpj")]
    public required string Cnpj { get; set; }

    [JsonProperty("nome")]
    public required string Nome { get; set; }

    [JsonProperty("fantasia")]
    public required string Fantasia { get; set; }

    [JsonProperty("cep")]
    public required string Cep { get; set; }

    [JsonProperty("uf")]
    public required string Uf { get; set; }

    [JsonProperty("municipio")]
    public required string Municipio { get; set; }

    [JsonProperty("bairro")]
    public required string Bairro { get; set; }

    [JsonProperty("tipo_logradouro")]
    public required object TipoLogradouro { get; set; }

    [JsonProperty("logradouro")]
    public required string Logradouro { get; set; }

    [JsonProperty("numero")]
    public required string Numero { get; set; }

    [JsonProperty("complemento")]
    public required string Complemento { get; set; }

    [JsonProperty("telefone")]
    public required string Telefone { get; set; }

    [JsonProperty("email")]
    public required string Email { get; set; }

    [JsonProperty("capital_social")]
    public required string CapitalSocial { get; set; }

    [JsonProperty("data_situacao")]
    public required string DataSituacao { get; set; }

    [JsonProperty("data_situacao_especial")]
    public required string DataSituacaoEspecial { get; set; }

    [JsonProperty("abertura")]
    public required DateTime Abertura { get; set; }

    [JsonProperty("motivo_situacao")]
    public required string MotivoSituacao { get; set; }

    [JsonProperty("sigla_natureza_juridica")]
    public required string SiglaNaturezaJuridica { get; set; }

    [JsonProperty("natureza_juridica")]
    public required string NaturezaJuridica { get; set; }

    [JsonProperty("situacao")]
    public required string Situacao { get; set; }

    [JsonProperty("situacao_especial")]
    public required string SituacaoEspecial { get; set; }

    [JsonProperty("tipo")]
    public required string Tipo { get; set; }

    [JsonProperty("atividade_principal")]
    public required AtividadePrincipal[] AtividadePrincipal { get; set; }

    [JsonProperty("atividades_secundarias")]
    public required AtividadesSecundaria[] AtividadesSecundarias { get; set; }

    [JsonProperty("qsa")]
    public required Qsa[] Qsa { get; set; }

    [JsonProperty("ultima_atualizacao")]
    public required string UltimaAtualizacao { get; set; }

    [JsonProperty("efr")]
    public required string Efr { get; set; }

    [JsonProperty("extra")]
    public object? Extra { get; set; }

    [JsonProperty("porte")]
    public required string Porte { get; set; }

    [JsonProperty("ibge")]
    public required Ibge Ibge { get; set; }

    [JsonProperty("cnpjs_do_grupo")]
    public required CnpjDoGrupo[] CnpjsDoGrupo { get; set; }

    [JsonProperty("inscricao_municipal")]
    public required string InscricaoMunicipal { get; set; }

    [JsonProperty("coordinates")]
    public required Coordinates Coordinates { get; set; }

    [JsonProperty("version")]
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
            AtividadePrincipal = new CnaeResult()
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
    [JsonProperty("codigo_municipio")]
    public required string CodigoMunicipio { get; set; }

    [JsonProperty("codigo_uf")]
    public required string CodigoUf { get; set; }
}

public class Coordinates
{
    [JsonProperty("latitude")]
    public required string Latitude { get; set; }

    [JsonProperty("longitude")]
    public required string Longitude { get; set; }
}

public class AtividadePrincipal
{
    [JsonProperty("code")]
    public required string Code { get; set; }

    [JsonProperty("text")]
    public required string Text { get; set; }
}

public class AtividadesSecundaria
{
    [JsonProperty("code")]
    public required string Code { get; set; }

    [JsonProperty("text")]
    public required string Text { get; set; }
}

public class Qsa
{
    [JsonProperty("qual")]
    public required string Qual { get; set; }

    [JsonProperty("qual_rep_legal")]
    public required string QualRepLegal { get; set; }

    [JsonProperty("nome_rep_legal")]
    public required string NomeRepLegal { get; set; }

    [JsonProperty("pais_origem")]
    public required string PaisOrigem { get; set; }

    [JsonProperty("nome")]
    public required string Nome { get; set; }

    [JsonProperty("faixa_etaria")]
    public required object FaixaEtaria { get; set; }
}

public class CnpjDoGrupo
{
    [JsonProperty("cnpj")]
    public required string Cnpj { get; set; }

    [JsonProperty("uf")]
    public required string Uf { get; set; }

    [JsonProperty("tipo")]
    public required string Tipo { get; set; }
}