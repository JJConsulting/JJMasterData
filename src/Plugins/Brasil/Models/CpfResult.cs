using System.Collections.Generic;

namespace JJMasterData.Brasil.Models;

using Newtonsoft.Json;

public class CpfResult
{
    [JsonProperty("Nome_Da_Pf")]
    public required string NomeDaPf { get; set; }

    [JsonProperty("Situacao_Cadastral")]
    public required string SituacaoCadastral { get; set; }

    [JsonProperty("Comprovante_Emitido")]
    public required string ComprovanteEmitido { get; set; }

 
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "Nome_Da_Pf", NomeDaPf },
            { "Situacao_Cadastral", SituacaoCadastral },
            { "Comprovante_Emitido", ComprovanteEmitido }
        };
    }
}
