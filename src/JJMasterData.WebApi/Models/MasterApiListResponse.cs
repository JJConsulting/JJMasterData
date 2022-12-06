using System.Data;
using System.Runtime.Serialization;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.WebApi.Models;

[Serializable]
[DataContract]
public class MasterApiListResponse
{
    /// <summary>
    /// Quantidade total de registros no banco
    /// </summary>
    [DataMember(Name = "tot")]
    public int Tot { get; set; }

    /// <summary>
    /// Tabela com os dados da pesquisa
    /// </summary>
    [DataMember(Name = "fields")]
    public Dictionary<string, object?>[]? Fields { get; set; }


    public void SetDataTableValues(Metadata metadata, DataTable? dataTable)
    {
        if (dataTable == null)
            return;
        
        var list = new List<Dictionary<string, object?>>();
        foreach(DataRow row in dataTable.Rows)
        {
            var cols = new Dictionary<string, object?>();
            foreach(var field in metadata.Table.Fields)
            {
                string fieldName = metadata.Api.GetFieldNameParsed(field.Name);
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