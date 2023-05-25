using System.Data;
using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary;
using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;


public class MasterApiListResponse
{
    /// <summary>
    /// Quantidade total de registros no banco
    /// </summary>
    [JsonProperty("tot")]
    public int Tot { get; set; }

    /// <summary>
    /// Tabela com os dados da pesquisa
    /// </summary>
    [JsonProperty("fields")]
    public Dictionary<string, object?>[]? Fields { get; set; }


    public void SetDataTableValues(FormElement formElement, DataTable? dataTable)
    {
        if (dataTable == null)
            return;
        
        var list = new List<Dictionary<string, object?>>();
        foreach(DataRow row in dataTable.Rows)
        {
            var cols = new Dictionary<string, object?>();
            foreach(var field in formElement.Fields)
            {
                string fieldName = formElement.ApiOptions.GetFieldNameParsed(field.Name);
                object val = row[field.Name];
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