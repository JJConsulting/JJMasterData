using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using System.Collections.Generic;
using System.Data;

namespace JJMasterData.Core.Web.Components;

internal class SearchBoxData
{

    public FormElementDataItem DataItem { get; }

    public ExpressionOptions ExpressionOptions { get; }

    public SearchBoxData(FormElementDataItem dataItem, ExpressionOptions expressionOptions)
    {
        DataItem = dataItem;
        ExpressionOptions = expressionOptions;
    }
    

    public IList<DataItemValue> GetValues(string searchText, string searchId)
    {
        if (DataItem == null)
            return null;

        if (DataItem.Command == null || string.IsNullOrEmpty(DataItem.Command.Sql))
            return DataItem.Items;

        var values = new List<DataItemValue>();
        var sql = GetSqlParsed(searchText, searchId);

        DataTable dt = ExpressionOptions.EntityRepository.GetDataTable(sql);
        foreach (DataRow row in dt.Rows)
        {
            var item = new DataItemValue();
            item.Id = row[0].ToString();
            item.Description = row[1].ToString().Trim();
            if (DataItem.ShowImageLegend)
            {
                item.Icon = (IconType)int.Parse(row[2].ToString());
                item.ImageColor = row[3].ToString();
            }

            values.Add(item);
        }
        

        return values;
    }

    private string GetSqlParsed(string searchText, string searchId)
    {
        var userVaLues = ExpressionOptions.UserValues;
        string sql = DataItem.Command.Sql;
        if (sql.Contains("{"))
        {
            if (searchId != null)
            {
                if (!userVaLues.Contains("search_id"))
                    userVaLues.Add("search_id", StringManager.ClearText(searchId));
            }

            if (searchText != null)
            {
                if (!userVaLues.Contains("search_text"))
                    userVaLues.Add("search_text", StringManager.ClearText(searchText));
            }

            var exp = new ExpressionManager(userVaLues, ExpressionOptions.EntityRepository);
            sql = exp.ParseExpression(sql, ExpressionOptions.PageState, false, ExpressionOptions.FormValues);
        }

        return sql;
    }
}