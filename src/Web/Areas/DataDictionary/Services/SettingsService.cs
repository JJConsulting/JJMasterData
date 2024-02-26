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
    private IWritableOptions<MasterDataCommonsOptions> CommonsWritableOptions { get; } = commonsWritableOptions;
    private IWritableOptions<MasterDataCoreOptions> CoreWritableOptions { get; } = coreWritableOptions;
    private IWritableOptions<MasterDataWebOptions> WebWritableOptions { get; } = webWritableOptions;

    public async Task SaveOptions(SettingsViewModel model)
    {
        if (IsValid)
        {
            await CommonsWritableOptions.UpdateAsync(options =>
            {
                options.ConnectionString = model.CommonsOptions.ConnectionString;
                options.ConnectionProvider = model.CommonsOptions.ConnectionProvider;
                options.ReadProcedurePattern = model.CommonsOptions.ReadProcedurePattern;
                options.WriteProcedurePattern = model.CommonsOptions.WriteProcedurePattern;
                options.SecretKey = model.CommonsOptions.SecretKey;
                options.LocalizationTableName = model.CommonsOptions.LocalizationTableName;
            });
            await CoreWritableOptions.UpdateAsync(options =>
            {
                options.DataDictionaryTableName = model.CoreOptions.DataDictionaryTableName;
                options.ExportationFolderPath = model.CoreOptions.ExportationFolderPath;
                options.AuditLogTableName = model.CoreOptions.AuditLogTableName;
            });
            await WebWritableOptions.UpdateAsync(options =>
            {
                options.CustomBootstrapPath = model.WebOptions.CustomBootstrapPath;
                options.UseAdvancedModeAtExpressions = model.WebOptions.UseAdvancedModeAtExpressions;
                options.LayoutPath = model.WebOptions.LayoutPath;
                options.ModalLayoutPath = model.WebOptions.ModalLayoutPath;
                options.EnableBundleAndMinification = model.WebOptions.EnableBundleAndMinification;
            });
        }
    }

    public async Task<SettingsViewModel> GetViewModel()
    {
        var connectionString = CommonsWritableOptions.Value.ConnectionString;
        var connectionProvider = CommonsWritableOptions.Value?.ConnectionProvider ?? DataAccessProvider.SqlServer;
        
        var connectionResult = await GetConnectionResultAsync(connectionString, connectionProvider);
        var viewModel = new SettingsViewModel
        {
            ConnectionString = new ConnectionString(connectionString),
            CommonsOptions = CommonsWritableOptions.Value!,
            CoreOptions = CoreWritableOptions.Value!,
            WebOptions = WebWritableOptions.Value!,
            FilePath = CoreWritableOptions.FilePath,

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