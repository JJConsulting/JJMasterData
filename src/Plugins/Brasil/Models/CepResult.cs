using System.Collections.Generic;

namespace JJMasterData.Brasil.Models;


public class CepResult
{
    public string Logradouro { get; set; }
    public string Complemento { get; set; }
    public string Bairro { get; set; }
    public string Localidade { get; set; }
    public string Uf { get; set; }
    public string Unidade { get; set; }
    
    public Dictionary<string, string> ToDictionary()
    {
        var dictionary = new Dictionary<string, string>
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