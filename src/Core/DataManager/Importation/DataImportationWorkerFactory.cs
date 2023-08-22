using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Imports;
using JJMasterData.Core.DataManager.Services.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components.Importation;

public class DataImportationWorkerFactory
{
    public ILoggerFactory LoggerFactory { get; }

    public IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public IFieldValuesService FieldValuesService { get; }

    public IEntityRepository EntityRepository { get; }

    public IExpressionsService ExpressionsService { get; }

    public IFormService FormService { get; }
    
    public DataImportationWorkerFactory(IFormService formService, IExpressionsService expressionsService, IEntityRepository entityRepository, IFieldValuesService fieldValuesService, IStringLocalizer<JJMasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
    {
        FormService = formService;
        ExpressionsService = expressionsService;
        EntityRepository = entityRepository;
        FieldValuesService = fieldValuesService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public DataImportationWorker Create(DataImportationContext context)
    {
        return new DataImportationWorker(context, FormService, ExpressionsService, EntityRepository, FieldValuesService,
            StringLocalizer, LoggerFactory.CreateLogger<DataImportationWorker>());
    }
}