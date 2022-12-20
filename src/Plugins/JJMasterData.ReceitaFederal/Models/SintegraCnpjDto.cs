namespace JJMasterData.ReceitaFederal.Models;

internal class SintegraCnpjDto
{
    public string code { get; set; }
    public string status { get; set; }
    public string message { get; set; }
    public string cnpj { get; set; }
    public string nome { get; set; }
    public string fantasia { get; set; }
    public string cep { get; set; }
    public string uf { get; set; }
    public string municipio { get; set; }
    public string bairro { get; set; }
    public object tipo_logradouro { get; set; }
    public string logradouro { get; set; }
    public string numero { get; set; }
    public string complemento { get; set; }
    public string telefone { get; set; }
    public string email { get; set; }
    public string capital_social { get; set; }
    public string data_situacao { get; set; }
    public string data_situacao_especial { get; set; }
    public string abertura { get; set; }
    public string motivo_situacao { get; set; }
    public string sigla_natureza_juridica { get; set; }
    public string natureza_juridica { get; set; }
    public string situacao { get; set; }
    public string situacao_especial { get; set; }
    public string tipo { get; set; }
    public Atividade_Principal[] atividade_principal { get; set; }
    public Atividades_Secundarias[] atividades_secundarias { get; set; }
    public Qsa[] qsa { get; set; }
    public string ultima_atualizacao { get; set; }
    public string efr { get; set; }
    public object extra { get; set; }
    public string porte { get; set; }
    public Ibge ibge { get; set; }
    public Cnpjs_Do_Grupo[] cnpjs_do_grupo { get; set; }
    public string inscricao_municipal { get; set; }
    public Coordinates coordinates { get; set; }
    public string version { get; set; }
}

public class Ibge
{
    public string codigo_municipio { get; set; }
    public string codigo_uf { get; set; }
}

public class Coordinates
{
    public string latitude { get; set; }
    public string longitude { get; set; }
}

public class Atividade_Principal
{
    public string code { get; set; }
    public string text { get; set; }
}

public class Atividades_Secundarias
{
    public string code { get; set; }
    public string text { get; set; }
}

public class Qsa
{
    public string qual { get; set; }
    public string qual_rep_legal { get; set; }
    public string nome_rep_legal { get; set; }
    public string pais_origem { get; set; }
    public string nome { get; set; }
    public object faixa_etaria { get; set; }
}

public class Cnpjs_Do_Grupo
{
    public string cnpj { get; set; }
    public string uf { get; set; }
    public string tipo { get; set; }
}