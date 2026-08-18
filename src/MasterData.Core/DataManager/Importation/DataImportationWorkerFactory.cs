using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager.Importation;

public class DataImportationWorkerFactory(FormService formService, ExpressionsService expressionsService,
    IEntityRepository entityRepository, FieldValuesService fieldValuesService,
    IFileStorage fileStorage,
    IStringLocalizer<MasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
{
    public DataImportationWorker Create(DataImportationContext context)
    {
        return new DataImportationWorker(context, formService, expressionsService, entityRepository, fieldValuesService, fileStorage,
            stringLocalizer, loggerFactory.CreateLogger<DataImportationWorker>());
    }
}
