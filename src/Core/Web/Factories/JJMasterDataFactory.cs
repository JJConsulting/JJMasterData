using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DI;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class JJMasterDataFactory
{
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;
    private readonly IHttpContext _httpContext;

    public JJMasterDataFactory(IEntityRepository entityRepository,
                               IDataDictionaryRepository dataDictionaryRepository,
                               IHttpContext httpContext)
    {
        _entityRepository = entityRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
        _httpContext = httpContext;
    }

    public static JJMasterDataFactory GetInstance()
    {
        return new JJMasterDataFactory(JJService.EntityRepository,
                                       JJServiceCore.DataDictionaryRepository,
                                       JJHttpContext.GetInstance());

    }


    public JJSearchBox CreateJJSearchBox(string dictionaryName, string fieldName, PageState pageState, IDictionary userValues)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            return null;

        IDictionary formValues = null;
        var element = _dataDictionaryRepository.GetMetadata(dictionaryName);
        var dataItem = element.Fields[fieldName].DataItem;
        if (dataItem == null)
            throw new ArgumentNullException(nameof(dataItem));

        if (dataItem.HasSqlExpression())
        {
            var expression = new ExpressionManager(userValues, _entityRepository);
            var fieldManager = new FieldManager(element, expression);
            var formRequest = new FormValues(fieldManager);
            var dbValues = formRequest.GetDatabaseValuesFromPk(element);
            formValues = formRequest.GetFormValues(pageState, dbValues, true);
        }

        var field = element.Fields[fieldName];
        var expOptions = new ExpressionOptions(userValues, formValues, pageState, _entityRepository);
        return JJSearchBox.GetInstance(field, expOptions, null, dictionaryName);
    }

}