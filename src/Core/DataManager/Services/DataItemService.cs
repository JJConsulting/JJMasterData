#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
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
    
    public async Task<string> GetSelectedValueAsync(FormElementField field, FormStateData formStateData, string searchText)
    {
        if (HttpContext.IsPost)
        {
            string? value = HttpContext.Request.Form(field.Name);
            if (value is not null)
                return value;
        }
        
        var list = await GetValuesAsync(field.DataItem!, formStateData, searchText,null).ToListAsync();
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

    public async IAsyncEnumerable<DataItemValue> GetValuesAsync(
        FormElementDataItem dataItem,
        FormStateData formStateData,
        string? searchText,
        string? searchId)
    {
        if ((dataItem.Command == null || string.IsNullOrEmpty(dataItem.Command.Sql)) && dataItem.Items != null)
        {
            foreach (var item in dataItem.Items)
            {
                yield return item;
            }
            yield break;
        }

        var sql = GetSqlParsed(dataItem, formStateData, searchText, searchId);

        var dictionary = await EntityRepository.GetDictionaryListAsync(new DataAccessCommand(sql!));
        
        foreach (var row in dictionary)
        {
            var item = new DataItemValue
            {
                Id = row.ElementAt(0).Value?.ToString(),
                Description = row.ElementAt(1).Value?.ToString()!.Trim()
            };
            if (dataItem.ShowImageLegend)
            {
                item.Icon = (IconType)int.Parse(row.ElementAt(2).Value?.ToString() ?? string.Empty);
                item.ImageColor = row.ElementAt(3).Value?.ToString();
            }

            if (searchText == null || item.Description!.ToLower().Contains(searchText))
            {
                yield return item;
            }
        }
    }

    private string? GetSqlParsed(FormElementDataItem dataItem, FormStateData formStateData, string? searchText, string? searchId)
    {
        var sql = dataItem.Command!.Sql;
        if (sql.Contains("{"))
        {
            if (searchId != null)
            {
                if (formStateData.UserValues != null && !formStateData.UserValues.ContainsKey("search_id"))
                    formStateData.UserValues.Add("search_id", StringManager.ClearText(searchId));
            }

            if (searchText != null)
            {
                if (formStateData.UserValues != null && !formStateData.UserValues.ContainsKey("search_text"))
                    formStateData.UserValues.Add("search_text", StringManager.ClearText(searchText));
            }

            sql = ExpressionsService.ParseExpression(sql, formStateData, false);
        }

        return sql;
    }
}