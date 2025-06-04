using System.Text.Json.Serialization;
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


    public void SetData(FormElement formElement, IEnumerable<Dictionary<string, object?>> data)
    {
        var list = new List<Dictionary<string, object?>>();
        foreach (var row in data)
        {
            var cols = new Dictionary<string, object?>();
            foreach (var field in formElement.Fields)
            {
                var fieldName = formElement.ApiOptions.GetJsonFieldName(field.Name);
                var val = row[field.Name];
                if (val == DBNull.Value)
                    cols.Add(fieldName, null);
                else
                    cols.Add(fieldName, val);
            }

            list.Add(cols);
        }

        Fields = list.ToArray();
    }
}