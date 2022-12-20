using System;

namespace JJMasterData.ReceitaFederal.Models;

[Serializable]
public class CpfInfo
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
    public CpfResult Result { get; set; }

    [Serializable]
    public class CpfResult
    {
        public string Nome_Da_Pf { get; set; }
        public string Situacao_Cadastral { get; set; }
        public string Comprovante_Emitido { get; set; }
    }

    internal static CpfInfo GetInstance(SintegraCpfDto dto)
    {
        var obj = new CpfInfo();
        obj.Message = dto.message;
        obj.Return = dto.status;

        if (!"0".Equals(dto.code))
        {
            obj.Return = "NOK";
            return obj;
        }
            
        obj.Result = new CpfResult();
        obj.Result.Nome_Da_Pf = dto.nome;
        obj.Result.Situacao_Cadastral = dto.situacao_cadastral;
        obj.Result.Comprovante_Emitido = dto.comprovante;

        return obj;
    }
}