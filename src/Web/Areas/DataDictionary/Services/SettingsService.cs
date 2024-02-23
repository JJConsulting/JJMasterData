using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Configuration.Options.Abstractions;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using JJMasterData.Web.Configuration.Options;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.DataDictionary.Services;

public class SettingsService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IWritableOptions<MasterDataCommonsOptions> commonsWritableOptions,
        IWritableOptions<MasterDataCoreOptions> coreWritableOptions,
        IWritableOptions<MasterDataWebOptions> webWritableOptions
        )
    : BaseService(validationDictionary, dataDictionaryRepository, stringLocalizer)
{
    public async Task SaveOptions(SettingsViewModel model)
    {
        if (IsValid)
        {
            await commonsWritableOptions.UpdateAsync(options =>
            {
                options.ConnectionString = model.ConnectionString.ToString();
                options.ConnectionProvider = model.ConnectionProvider;
            });
            await coreWritableOptions.UpdateAsync(options =>
            {
                options.DataDictionaryTableName = model.DataDictionaryTableName!;
            });
            await webWritableOptions.UpdateAsync(options =>
            {
                options.CustomBootstrapPath = model.CustomBootstrapPath;
                options.UseAdvancedModeAtExpressions = model.UseAdvancedModeAtExpressions;
            });
        }
    }

    public async Task<SettingsViewModel> GetViewModel()
    {
        var connectionString = commonsWritableOptions.Value.ConnectionString;
        var connectionProvider = commonsWritableOptions.Value?.ConnectionProvider ?? DataAccessProvider.SqlServer;
        
        var connectionResult = await GetConnectionResultAsync(connectionString, connectionProvider);
        var viewModel = new SettingsViewModel
        {
            ConnectionString = new ConnectionString(connectionString),
            ConnectionProvider = connectionProvider,
            UseAdvancedModeAtExpressions = webWritableOptions.Value.UseAdvancedModeAtExpressions,
            DataDictionaryTableName = coreWritableOptions.Value.DataDictionaryTableName,
            CustomBootstrapPath = webWritableOptions.Value.CustomBootstrapPath,
            FilePath = coreWritableOptions.FilePath,

        };

        if (!viewModel.PathExists)
            AddError(nameof(SettingsViewModel.FilePath),
                StringLocalizer["{0} does not exists.", viewModel.FilePath ?? "File path"]);

        if (!connectionResult.IsConnectionSuccessful.GetValueOrDefault())
            AddError(nameof(SettingsViewModel.ConnectionString), connectionResult.ErrorMessage);
        
        viewModel.ValidationSummary = GetValidationSummary();

        return viewModel;
    }
    

    public static async Task<ConnectionResult> GetConnectionResultAsync(
        string? connectionString, 
        DataAccessProvider provider, 
        CancellationToken cancellationToken = default)
    {
        if (connectionString == null)
            return new ConnectionResult(false, "Connection String not found");

        var dataAccess = new DataAccess(connectionString, provider)
        {
            TimeOut = 5
        };
        var result = await dataAccess.TryConnectionAsync(cancellationToken);
        return new ConnectionResult(result.Item1, result.Item2);
    }
}