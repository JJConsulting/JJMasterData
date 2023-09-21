using System.Collections.Generic;

namespace JJMasterData.ReceitaFederal.Models;


public class CnpjInfo
{
    /// <summary>
    /// Indica a situação da requisição. Valores possíveis: OK, NOK, ERROR
    /// </summary>
    /// <remarks>
    /// <para>OK    - API Retornou dados com sucesso.</para>
    /// <para>NOK   - API Retornou mensagem de erro. </para>
    /// <para>ERROR - Um erro ocorreu ao tentar chamar a API.</para>
    /// </remarks>
    public string Return { get; set; }

    /// <summary>
    /// Mensagem explicativa indicando erro. Válido apenas quando o campo status é diferente de OK.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Detalhes do Objeto
    /// </summary>
    public CnpjResult Result { get; set; }
        
    
    public class CnpjResult
    {
        /// <summary>
        /// Razão social.
        /// </summary>
        public string Nome { get; set; }

        /// <summary>
        /// Nome fantasia.
        /// </summary>
        public string Fantasia { get; set; }

        /// <summary>
        /// Endereço
        /// </summary>
        public string Logradouro { get; set; }

        /// <summary>
        /// Número.
        /// </summary>
        public string Numero { get; set; }

        /// <summary>
        /// Complemento.
        /// </summary>
        public string Complemento { get; set; }

        /// <summary>
        /// CEP sem mascara no formato 00000000.
        /// </summary>
        public string Cep { get; set; }

        /// <summary>
        /// Nome do Bairro.
        /// </summary>
        public string Bairro { get; set; }

        /// <summary>
        /// Nome do Município.
        /// </summary>
        public string Municipio { get; set; }

        /// <summary>
        /// Sigla da Unidade da Federação.
        /// </summary>
        public string UF { get; set; }

        /// <summary>
        /// E-Mail.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Telefone.
        /// </summary>
        public string Telefone { get; set; }

        /// <summary>
        /// Situação.
        /// </summary>
        public string Situacao { get; set; }

        /// <summary>
        /// CNAE
        /// </summary>
        public CnaeInfo Atividade_principal { get; set; }

        /// <summary>
        /// Capital Social
        /// </summary>
        public string Capital_social { get; set; }

        /// <summary>
        /// Quadro Sócios
        /// </summary>
        public string[] Quadro_socios { get; set; }

        /// <summary>
        /// Data da Abertura
        /// </summary>
        public string Abertura { get; set; }
    }

    internal static CnpjInfo GetInstance(SintegraCnpjDto dto)
    {
        var obj = new CnpjInfo();
        obj.Return = dto.status;
        obj.Message = dto.message;

        if (!"0".Equals(dto.code))
        {
            obj.Return = "NOK";
            return obj;
        }

        obj.Result = new CnpjResult();
        obj.Result.Nome = dto.nome;
        obj.Result.Fantasia = dto.fantasia;
        obj.Result.Logradouro = dto.logradouro;
        obj.Result.Numero = dto.numero;
        obj.Result.Complemento = dto.complemento;
        obj.Result.Cep = dto.cep;
        obj.Result.Bairro = dto.bairro;
        obj.Result.Municipio = dto.municipio;
        obj.Result.UF = dto.uf;
        obj.Result.Email = dto.email;
        obj.Result.Telefone = dto.telefone;
        obj.Result.Situacao = dto.situacao;
        obj.Result.Capital_social = dto.capital_social;
        obj.Result.Abertura = dto.abertura;

        if (dto.atividade_principal != null && dto.atividade_principal.Length > 0)
        {
            var cnae = new CnaeInfo();
            cnae.Text = dto.atividade_principal[0].text;
            cnae.Code = dto.atividade_principal[0].code;
            obj.Result.Atividade_principal = cnae;
        }

        if (dto.qsa != null && dto.qsa.Length > 0)
        {
            var socios = new List<string>();
            foreach(var item in dto.qsa)
            {
                socios.Add(item.nome);
            }

            obj.Result.Quadro_socios = socios.ToArray();
        }

        return obj;
    }
}