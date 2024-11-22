using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace JJMasterData.Brasil.Models;


public class CepResult
{
    public required string Logradouro { get; set; }
    public string? Complemento { get; set; }
    public required string Bairro { get; set; }
    public required string Localidade { get; set; }
    public required string Uf { get; set; }
    public string? Unidade { get; set; }
    
    [JsonPropertyName("erro")]
    internal bool Erro { get; set; }
    
    public static CepResult FromJson(string json)
    {
        return JsonSerializer.Deserialize<CepResult>(json, JsonSerializerOptions.Default)!;
    }
    
    public Dictionary<string, object?> ToDictionary()
    {
        var dictionary = new Dictionary<string, object?>
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