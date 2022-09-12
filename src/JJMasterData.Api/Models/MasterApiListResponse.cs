using System.Data;
using System.Runtime.Serialization;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary.DictionaryDAL;

namespace JJMasterData.Api.Models;

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
    public Dictionary<string, object>[] Fields { get; set; }


    public void SetDataTableValues(DicParser dic, DataTable dt)
    {
        if (dt == null)
            return;
        
        var list = new List<Dictionary<string, object>>();
        foreach(DataRow row in dt.Rows)
        {
            var cols = new Dictionary<string, object>();
            foreach(ElementField field in dic.Table.Fields)
            {
                string fieldName = dic.Api.GetFieldNameParsed(field.Name);
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