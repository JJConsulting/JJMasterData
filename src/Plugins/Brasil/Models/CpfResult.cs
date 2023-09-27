namespace JJMasterData.Brasil.Models;

using Newtonsoft.Json;

public class CpfResult
{
    [JsonProperty("Nome_Da_Pf")]
    public string NomeDaPf { get; set; }

    [JsonProperty("Situacao_Cadastral")]
    public string SituacaoCadastral { get; set; }

    [JsonProperty("Comprovante_Emitido")]
    public string ComprovanteEmitido { get; set; }
}
