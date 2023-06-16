using System.Data;
using JJMasterData.BlazorClient.Models;
using JJMasterData.BlazorClient.Services.Abstractions;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.BlazorClient.Services;

public record SearchBoxContext(string? SearchId, IDictionary<string, dynamic?> Values,
    IDictionary<string, dynamic?>? UserValues, PageState PageState);

public class SearchBoxService : ISearchBoxService
{
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }

    public SearchBoxService(IEntityRepository entityRepository, IExpressionsService expressionsService)
    {
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
    }

    public async Task<IEnumerable<DataItemValue>> GetValues(FormElementDataItem dataItem,
        string? text,
        SearchBoxContext searchBoxContext)
    {
        if (dataItem.Command == null || string.IsNullOrEmpty(dataItem.Command.Sql))
            return dataItem.Items;

        var values = new List<DataItemValue>();
        var sql = GetSqlParsed(dataItem, text, searchBoxContext);

        var dt = await EntityRepository.GetDataTableAsync(sql);
        foreach (DataRow row in dt.Rows)
        {
            var item = new DataItemValue
            {
                Id = row[0].ToString(),
                Description = row[1].ToString()?.Trim()
            };
            if (dataItem.ShowImageLegend)
            {
                item.Icon = (IconType)int.Parse(row[2].ToString() ?? string.Empty);
                item.ImageColor = row[3].ToString();
            }

            values.Add(item);
        }


        return values;
    }

    private string GetSqlParsed(FormElementDataItem dataItem, string? text, SearchBoxContext searchBoxContext)
    {
        var (searchId, values, userValues, pageState) = searchBoxContext;

        string sql = dataItem.Command.Sql;
        if (sql.Contains("{"))
        {
            if (searchId != null)
            {
                if (searchBoxContext.UserValues != null && !searchBoxContext.UserValues.ContainsKey("search_id"))
                    searchBoxContext.UserValues.Add("search_id", StringManager.ClearText(searchId));
            }

            if (text != null)
            {
                if (searchBoxContext.UserValues != null && !searchBoxContext.UserValues.ContainsKey("search_text"))
                    searchBoxContext.UserValues.Add("search_text", StringManager.ClearText(text));
            }

            sql = ExpressionsService.ParseExpression(sql, pageState, false,
                values, userValues);
        }

        return sql;
    }
}