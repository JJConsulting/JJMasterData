using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.DI;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class JJMasterDataFactory
{
    private readonly IExpressionsService _expressionsService;
    private readonly IFormValuesService _formValuesService;
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;
    private readonly IHttpContext _httpContext;

    public JJMasterDataFactory(IEntityRepository entityRepository,
                               IDataDictionaryRepository dataDictionaryRepository,
                               IExpressionsService expressionsService,
                               IFormValuesService formValuesService,
                               IHttpContext httpContext)
    {
        _expressionsService = expressionsService;
        _formValuesService = formValuesService;
        _entityRepository = entityRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
        _httpContext = httpContext;
    }

    
    public static JJMasterDataFactory GetInstance()
    {
        return new JJMasterDataFactory(JJService.EntityRepository,
                                       JJServiceCore.DataDictionaryRepository,
                                       JJService.Provider.GetScopedDependentService<IExpressionsService>(),
                                       JJService.Provider.GetScopedDependentService<IFormValuesService>(),
                                       JJHttpContext.GetInstance());

    }


    public JJSearchBox CreateJJSearchBox(string dictionaryName, string fieldName, PageState pageState, IDictionary<string,dynamic>userValues)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            return null;

        IDictionary<string,dynamic>formValues = null;
        var formElement = _dataDictionaryRepository.GetMetadata(dictionaryName);
        var dataItem = formElement.Fields[fieldName].DataItem;
        if (dataItem == null)
            throw new ArgumentNullException(nameof(dataItem));

        if (dataItem.HasSqlExpression())
        {
            formValues = _formValuesService.GetFormValuesWithMergedValues(formElement,pageState, true).GetAwaiter().GetResult();
        }

        var field = formElement.Fields[fieldName];
        var expOptions = new ExpressionOptions(userValues, formValues, pageState, _entityRepository);
        return JJSearchBox.GetInstance(field, expOptions, null, dictionaryName);
    }

}