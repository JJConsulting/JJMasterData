namespace JJMasterData.ReceitaFederal.Models;


public class CepInfo
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
    public CepResult Result { get; set; }

    
    public class CepResult
    {
        public string Logradouro { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Localidade { get; set; }
        public string Uf { get; set; }
        public string Unidade { get; set; }
    }
}