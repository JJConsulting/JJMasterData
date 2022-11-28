using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Options;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Models.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Services;

public class OptionsService : BaseService
{
    private IWritableOptions<ConnectionStrings>? ConnectionStringsWritableOptions { get; }
    private IWritableOptions<ConnectionProviders>? ConnectionProvidersWritableOptions { get; }
    internal IWritableOptions<JJMasterDataOptions>? JJMasterDataWritableOptions { get; }

    public OptionsService(IValidationDictionary validationDictionary,
        IDictionaryRepository dictionaryRepository,
        IWritableOptions<ConnectionStrings>? connectionStringsWritableOptions = null,
        IWritableOptions<JJMasterDataOptions>? masterDataWritableOptions = null,
        IWritableOptions<ConnectionProviders>? connectionProvidersWritableOptions = null) 
        : base(validationDictionary, dictionaryRepository)
    {
        JJMasterDataWritableOptions = masterDataWritableOptions;
        ConnectionStringsWritableOptions = connectionStringsWritableOptions;
        ConnectionProvidersWritableOptions = connectionProvidersWritableOptions;
    }


    public static async Task<(bool, string)> TryConnectionAsync(string? connectionString)
    {
        var dataAccess = new DataAccess
        {
            ConnectionString = connectionString
        };

        return await dataAccess.TryConnectionAsync();
    }

    public async Task SaveOptions(OptionsViewModel model)
    {
        await ConnectionStringsWritableOptions!.UpdateAsync(options =>
        {
            options.ConnectionString = model.ConnectionString!.ToString();
        });

        await JJMasterDataWritableOptions!.UpdateAsync(options =>
        {
            options.BootstrapVersion = model.Options!.BootstrapVersion;
            options.Logger = model.Options.Logger;
        });


        await ConnectionProvidersWritableOptions!.UpdateAsync(options =>
        {
            options.ConnectionString = model.ConnectionProvider.GetDescription();
        });
    }

    public async Task<OptionsViewModel> GetViewModel(bool isFullscreen)
    {
        string? connection = JJMasterDataOptions.GetConnectionString();
        var connectionResult = await GetConnectionResultAsync(connection);
        var viewModel = new OptionsViewModel
        {
            ConnectionString = new ConnectionString(connection),
            Options = JJMasterDataWritableOptions?.Value,
            ConnectionProvider =
                DataAccessProvider.GetDataAccessProviderTypeFromString(JJMasterDataOptions.GetConnectionProvider()),
            FilePath = JJMasterDataWritableOptions?.FilePath,
            IsFullscreen = isFullscreen,
            IsConnectionSuccessful = connectionResult.IsConnectionSuccessful
        };

        if (!viewModel.PathExists)
            AddError(nameof(OptionsViewModel.FilePath),$"{viewModel.FilePath} does not exists.");
        
        if(!connectionResult.IsConnectionSuccessful.GetValueOrDefault())
            AddError(nameof(OptionsViewModel.ConnectionString), connectionResult.ErrorMessage);
        
        //TODO: Translate this.
        if(JJMasterDataWritableOptions == null)
            AddError(string.Empty,"You cannot save your options because I");
        
        viewModel.ValidationSummary = GetValidationSummary();
        
        return viewModel;
    }

    public static async Task<ConnectionResult> GetConnectionResultAsync(string? connectionString)
    {
        var result = await TryConnectionAsync(connectionString);
        return new ConnectionResult(result.Item1, result.Item2);
    }
}