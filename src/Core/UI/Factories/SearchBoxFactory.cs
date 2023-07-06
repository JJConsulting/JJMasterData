using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class SearchBoxFactory
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IFormValuesService FormValuesService { get; }
    private IHttpContext HttpContext { get; }

    public SearchBoxFactory(
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository, 
        IFormValuesService formValuesService,
        IHttpContext httpContext)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        FormValuesService = formValuesService;
        HttpContext = httpContext;
    }
    
    public JJSearchBox CreateSearchBox()
    {
        return new JJSearchBox(HttpContext);
    }
    
    internal JJSearchBox CreateSearchBox(FormElementField field, ExpressionOptions expOptions, object value, string dictionaryName)
    {
        var search = new JJSearchBox(expOptions, HttpContext)
        {
            DataItem = field.DataItem
        };
        search.Name = field.Name;
        search.FieldName = field.Name;
        search.DictionaryName = dictionaryName;
        search.SelectedValue = value?.ToString();
        search.Visible = true;
        search.AutoReloadFormFields = false;

        return search;
    }
    
    public async Task<JJSearchBox> CreateSearchBoxAsync(string dictionaryName, string fieldName, PageState pageState, IDictionary<string,dynamic>userValues)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            return null;

        IDictionary<string,dynamic>formValues = null;
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        var dataItem = formElement.Fields[fieldName].DataItem;
        if (dataItem == null)
            throw new ArgumentNullException(nameof(dataItem));

        if (dataItem.HasSqlExpression())
        {
            formValues = FormValuesService.GetFormValuesWithMergedValues(formElement,pageState, true).GetAwaiter().GetResult();
        }

        var field = formElement.Fields[fieldName];
        var expOptions = new ExpressionOptions(userValues, formValues, pageState);
        return CreateSearchBox(field, expOptions, null, dictionaryName);
    }
}