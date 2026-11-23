using System.Text.Json.Serialization;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Models;


namespace JJMasterData.WebApi.Models;

public class MasterApiListResponse
{
    /// <summary>
    /// Quantidade total de registros no banco
    /// </summary>
    [JsonPropertyName("tot")]
    public int TotalOfRecords { get; set; }

    /// <summary>
    /// Tabela com os dados da pesquisa
    /// </summary>
    [JsonPropertyName("fields")]
    public Dictionary<string, object?>[]? Fields { get; set; }

    public MasterApiListResponse(FormElement formElement, DictionaryListResult result)
    {
        var list = new List<Dictionary<string, object?>>();
        foreach (var row in result.Data)
        {
            var cols = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var field in formElement.Fields)
            {
                var fieldName = formElement.ApiOptions.GetJsonFieldName(field.Name);
                var val = row[field.Name];
                cols.Add(fieldName, val == DBNull.Value ? null : val);
            }

            list.Add(cols);
        }
        
        TotalOfRecords = result.TotalOfRecords;
        Fields = list.ToArray();
    }
}
