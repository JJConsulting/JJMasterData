using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JJMasterData.Brasil.Models;



public class CpfResult
{
    [JsonPropertyName("Nome_Da_Pf")]
    public required string NomeDaPf { get; set; }

    [JsonPropertyName("Situacao_Cadastral")]
    public required string SituacaoCadastral { get; set; }

    [JsonPropertyName("Comprovante_Emitido")]
    public required string ComprovanteEmitido { get; set; }
    
    public Dictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>
        {
            { nameof(NomeDaPf), NomeDaPf },
            { nameof(SituacaoCadastral), SituacaoCadastral },
            { nameof(ComprovanteEmitido), ComprovanteEmitido }
        };
    }
}
