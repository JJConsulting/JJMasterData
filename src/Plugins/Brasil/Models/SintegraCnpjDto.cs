using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Brasil.Models;

internal class SintegraCnpjDto
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("cnpj")]
    public string Cnpj { get; set; }

    [JsonProperty("nome")]
    public string Nome { get; set; }

    [JsonProperty("fantasia")]
    public string Fantasia { get; set; }

    [JsonProperty("cep")]
    public string Cep { get; set; }

    [JsonProperty("uf")]
    public string Uf { get; set; }

    [JsonProperty("municipio")]
    public string Municipio { get; set; }

    [JsonProperty("bairro")]
    public string Bairro { get; set; }

    [JsonProperty("tipo_logradouro")]
    public object TipoLogradouro { get; set; }

    [JsonProperty("logradouro")]
    public string Logradouro { get; set; }

    [JsonProperty("numero")]
    public string Numero { get; set; }

    [JsonProperty("complemento")]
    public string Complemento { get; set; }

    [JsonProperty("telefone")]
    public string Telefone { get; set; }

    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("capital_social")]
    public string CapitalSocial { get; set; }

    [JsonProperty("data_situacao")]
    public string DataSituacao { get; set; }

    [JsonProperty("data_situacao_especial")]
    public string DataSituacaoEspecial { get; set; }

    [JsonProperty("abertura")]
    public string Abertura { get; set; }

    [JsonProperty("motivo_situacao")]
    public string MotivoSituacao { get; set; }

    [JsonProperty("sigla_natureza_juridica")]
    public string SiglaNaturezaJuridica { get; set; }

    [JsonProperty("natureza_juridica")]
    public string NaturezaJuridica { get; set; }

    [JsonProperty("situacao")]
    public string Situacao { get; set; }

    [JsonProperty("situacao_especial")]
    public string SituacaoEspecial { get; set; }

    [JsonProperty("tipo")]
    public string Tipo { get; set; }

    [JsonProperty("atividade_principal")]
    public AtividadePrincipal[] AtividadePrincipal { get; set; }

    [JsonProperty("atividades_secundarias")]
    public AtividadesSecundarias[] AtividadesSecundarias { get; set; }

    [JsonProperty("qsa")]
    public Qsa[] Qsa { get; set; }

    [JsonProperty("ultima_atualizacao")]
    public string UltimaAtualizacao { get; set; }

    [JsonProperty("efr")]
    public string Efr { get; set; }

    [JsonProperty("extra")]
    public object Extra { get; set; }

    [JsonProperty("porte")]
    public string Porte { get; set; }

    [JsonProperty("ibge")]
    public Ibge Ibge { get; set; }

    [JsonProperty("cnpjs_do_grupo")]
    public CnpjsDoGrupo[] CnpjsDoGrupo { get; set; }

    [JsonProperty("inscricao_municipal")]
    public string InscricaoMunicipal { get; set; }

    [JsonProperty("coordinates")]
    public Coordinates Coordinates { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }
    
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
            UF = Uf,
            Email = Email,
            Telefone = Telefone,
            Situacao = Situacao,
            CapitalSocial = CapitalSocial,
            Abertura = Abertura
        };

        if (AtividadePrincipal != null && AtividadePrincipal.Length > 0)
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
            var socios = new List<string>();
            foreach(var item in Qsa)
            {
                socios.Add(item.Nome);
            }

            obj.QuadroSocios = socios.ToArray();
        }

        return obj;
    }

}

public class Ibge
{
    [JsonProperty("codigo_municipio")]
    public string CodigoMunicipio { get; set; }

    [JsonProperty("codigo_uf")]
    public string CodigoUf { get; set; }
}

public class Coordinates
{
    [JsonProperty("latitude")]
    public string Latitude { get; set; }

    [JsonProperty("longitude")]
    public string Longitude { get; set; }
}

public class AtividadePrincipal
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}

public class AtividadesSecundarias
{
    [JsonProperty("code")]
    public string Code { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}

public class Qsa
{
    [JsonProperty("qual")]
    public string Qual { get; set; }

    [JsonProperty("qual_rep_legal")]
    public string QualRepLegal { get; set; }

    [JsonProperty("nome_rep_legal")]
    public string NomeRepLegal { get; set; }

    [JsonProperty("pais_origem")]
    public string PaisOrigem { get; set; }

    [JsonProperty("nome")]
    public string Nome { get; set; }

    [JsonProperty("faixa_etaria")]
    public object FaixaEtaria { get; set; }
}

public class CnpjsDoGrupo
{
    [JsonProperty("cnpj")]
    public string Cnpj { get; set; }

    [JsonProperty("uf")]
    public string Uf { get; set; }

    [JsonProperty("tipo")]
    public string Tipo { get; set; }
}