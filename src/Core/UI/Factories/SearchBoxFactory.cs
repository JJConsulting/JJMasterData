using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

public class SearchBoxFactory
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IFormValuesService FormValuesService { get; }

    public SearchBoxFactory(IEntityRepository entityRepository,IDataDictionaryRepository dataDictionaryRepository, IFormValuesService formValuesService)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        FormValuesService = formValuesService;
    }
    
    internal JJSearchBox GetInstance(FormElementField field, ExpressionOptions expOptions, object value, string dictionaryName)
    {
        var search = new JJSearchBox(expOptions)
        {
            Name = field.Name,
            FieldName = field.Name,
            DictionaryName = dictionaryName,
            SelectedValue = value?.ToString(),
            Visible = true,
            AutoReloadFormFields = false,
            DataItem = field.DataItem
        };

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
        var expOptions = new ExpressionOptions(userValues, formValues, pageState, EntityRepository);
        return GetInstance(field, expOptions, null, dictionaryName);
    }
}