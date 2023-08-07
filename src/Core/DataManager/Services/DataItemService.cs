#nullable enable
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.DataManager.Services;

public class DataItemService : IDataItemService
{
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    private IHttpContext HttpContext { get; }

    public DataItemService(IEntityRepository entityRepository, IExpressionsService expressionsService, IHttpContext httpContext)
    {
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        HttpContext = httpContext;
    }
    
    public async Task<string> GetSelectedValueAsync(FormElementField field,string searchText, IDictionary<string,dynamic?> values, PageState pageState)
    {
        if (HttpContext.IsPost)
        {
            string? value = HttpContext.Request.Form(field.Name);
            if (value is not null)
                return value;
        }
        
        var list = await GetValuesAsync(field.DataItem!,searchText,null ,new SearchBoxContext(values,null,pageState)).ToListAsync();

        return list.First().Id;
    }

    public IEnumerable<DataItemResult> GetItems(FormElementDataItem dataItem, IEnumerable<DataItemValue> values)
    {
        foreach (var i in values.ToArray())
        {
            var description = dataItem.ShowImageLegend 
                ? $"{i.Description}|{i.Icon.GetCssClass()}|{i.ImageColor}"
                : i.Description;
            
            yield return new DataItemResult(i.Id, description);
        }
    }

    public async IAsyncEnumerable<DataItemValue> GetValuesAsync(FormElementDataItem dataItem,
        string? searchText,
        string? searchId,
        SearchBoxContext searchBoxContext)
    {
        if (dataItem.Command == null || string.IsNullOrEmpty(dataItem.Command.Sql))
        {
            foreach (var item in dataItem.Items)
            {
                yield return item;
            }
            yield break;
        }

        var sql = GetSqlParsed(dataItem, searchText, searchId, searchBoxContext);

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

            if (searchText == null || item.Description!.ToLower().Contains(searchText))
            {
                yield return item;
            }
        }
    }

    private string? GetSqlParsed(FormElementDataItem dataItem, string? searchText, string? searchId, SearchBoxContext searchBoxContext)
    {
        var ( values, userValues, pageState) = searchBoxContext;

        var sql = dataItem.Command.Sql;
        if (sql.Contains("{"))
        {
            if (searchId != null)
            {
                if (searchBoxContext.UserValues != null && !searchBoxContext.UserValues.ContainsKey("search_id"))
                    searchBoxContext.UserValues.Add("search_id", StringManager.ClearText(searchId));
            }

            if (searchText != null)
            {
                if (searchBoxContext.UserValues != null && !searchBoxContext.UserValues.ContainsKey("search_text"))
                    searchBoxContext.UserValues.Add("search_text", StringManager.ClearText(searchText));
            }

            sql = ExpressionsService.ParseExpression(sql, pageState, false,
                values, userValues);
        }

        return sql;
    }
}