using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Brasil.Models;


public class CepResult
{
    public required string Logradouro { get; set; }
    public string? Complemento { get; set; }
    public required string Bairro { get; set; }
    public required string Localidade { get; set; }
    public required string Uf { get; set; }
    public string? Unidade { get; set; }
    
    [JsonProperty("erro")]
    internal bool Erro { get; set; }
    
    public static CepResult FromJson(string json)
    {
        return JsonConvert.DeserializeObject<CepResult>(json)!;
    }
    
    public Dictionary<string, string?> ToDictionary()
    {
        var dictionary = new Dictionary<string, string?>
        {
            { nameof(Logradouro), Logradouro },
            { nameof(Complemento), Complemento },
            { nameof(Bairro), Bairro },
            { nameof(Localidade), Localidade },
            { nameof(Uf), Uf },
            { nameof(Unidade), Unidade }
        };
        return dictionary;
    }
}