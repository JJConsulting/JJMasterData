using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Importation;

public class DataImportationWorkerFactory(FormService formService, ExpressionsService expressionsService,
    IEntityRepository entityRepository, FieldValuesService fieldValuesService,
    IStringLocalizer<MasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
{
    public ILoggerFactory LoggerFactory { get; } = loggerFactory;

    public IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    public FieldValuesService FieldValuesService { get; } = fieldValuesService;

    public IEntityRepository EntityRepository { get; } = entityRepository;

    public ExpressionsService ExpressionsService { get; } = expressionsService;

    public FormService FormService { get; } = formService;

    public DataImportationWorker Create(DataImportationContext context)
    {
        return new DataImportationWorker(context, FormService, ExpressionsService, EntityRepository, FieldValuesService,
            StringLocalizer, LoggerFactory.CreateLogger<DataImportationWorker>());
    }
}