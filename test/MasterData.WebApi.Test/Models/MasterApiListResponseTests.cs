using System.Data;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.WebApi.Models;

namespace JJMasterData.WebApi.Test.Models;

public class MasterApiListResponseTests
{
    [Fact]
    public void FromList_PreservesJsonNamesOrderAndNullsWithoutChangingSource()
    {
        var element = new FormElement { Name = "Test" };
        element.Fields.Add(new FormElementField { Name = "ID" });
        element.Fields.Add(new FormElementField { Name = "NAME" });
        var source = new Dictionary<string, object?> { ["NAME"] = DBNull.Value, ["ID"] = 7 };
        var table = new DataTable();
        table.Columns.Add("NAME", typeof(string));
        table.Columns.Add("ID", typeof(int));
        table.Rows.Add(source["NAME"], source["ID"]);
        var result = DictionaryListResult.FromDataTable(table);
        var response = MasterApiListResponse.FromList(element, result);
        Assert.Equal(new[] { "id", "name" }, response.Fields![0].Keys);
        Assert.Equal(7, response.Fields[0]["id"]);
        Assert.Null(response.Fields[0]["name"]);
        Assert.Equal(DBNull.Value, source["NAME"]);
        Assert.Equal(result.TotalOfRecords, response.TotalOfRecords);
    }
}
