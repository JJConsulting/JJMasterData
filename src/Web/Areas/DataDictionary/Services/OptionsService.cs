using JJMasterData.Commons.Data;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Configuration.Options.Abstractions;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using System.Text;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Services;

public class OptionsService : BaseService
{
    private IWritableOptions<ConnectionStrings>? ConnectionStringsWritableOptions { get; }
    private IWritableOptions<ConnectionProviders>? ConnectionProvidersWritableOptions { get; }
    internal IWritableOptions<JJMasterDataCommonsOptions>? JJMasterDataWritableOptions { get; }

    public OptionsService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        IWritableOptions<ConnectionStrings>? connectionStringsWritableOptions = null,
        IWritableOptions<JJMasterDataCommonsOptions>? masterDataWritableOptions = null,
        IWritableOptions<ConnectionProviders>? connectionProvidersWritableOptions = null)
        : base(validationDictionary, dataDictionaryRepository, stringLocalizer)
    {
        JJMasterDataWritableOptions = masterDataWritableOptions;
        ConnectionStringsWritableOptions = connectionStringsWritableOptions;
        ConnectionProvidersWritableOptions = connectionProvidersWritableOptions;
    }


    public async Task SaveOptions(OptionsViewModel model)
    {
        ValidateWritableOptions();

        if (IsValid)
        {
            await ConnectionStringsWritableOptions!.UpdateAsync(options =>
            {
                options.ConnectionString = model.ConnectionString.ToString();
            });

            await ConnectionProvidersWritableOptions!.UpdateAsync(options =>
            {
                options.ConnectionString = model.ConnectionProvider.GetDescription();
            });
        }
    }

    public async Task<OptionsViewModel> GetViewModel(bool isFullscreen)
    {
        var stringConnection = ConnectionStringsWritableOptions?.Value.ConnectionString;
        var stringProvider = ConnectionProvidersWritableOptions?.Value.ConnectionString;
        var provider = DataAccessProvider.SqlServer;
        
        if (stringProvider != null)
            provider = Enum.Parse<DataAccessProvider>(stringProvider);

        var connectionResult = await GetConnectionResultAsync(stringConnection, provider);
        var viewModel = new OptionsViewModel
        {
            ConnectionString = new ConnectionString(stringConnection),
            ConnectionProvider = provider,
            FilePath = JJMasterDataWritableOptions?.FilePath,
            IsFullscreen = isFullscreen,
            IsConnectionSuccessful = connectionResult.IsConnectionSuccessful
        };

        if (!viewModel.PathExists)
            AddError(nameof(OptionsViewModel.FilePath),
                StringLocalizer["{0} does not exists.", viewModel.FilePath ?? "File path"]);

        if (!connectionResult.IsConnectionSuccessful.GetValueOrDefault())
            AddError(nameof(OptionsViewModel.ConnectionString), connectionResult.ErrorMessage);

        ValidateWritableOptions();

        viewModel.ValidationSummary = GetValidationSummary();

        return viewModel;
    }

    private void ValidateWritableOptions()
    {
        if (ConnectionProvidersWritableOptions == null || JJMasterDataWritableOptions == null ||
            ConnectionStringsWritableOptions == null)
            AddError("IWritableOptions",
                GetWritableOptionsErrorMessage());
    }

    private string GetWritableOptionsErrorMessage()
    {
        var message = new StringBuilder();
        message.AppendLine(StringLocalizer[
            "You cannot save your options because they don't use <a href=\"{0}\">IWritableOptions.</a>",
            "https://portal.jjconsulting.com.br/jjdoc/" +
            "lib/JJMasterData.Web.Models.Abstractions.IWritableOptions.html"]);
        message.AppendLine(StringLocalizer[
            "You can manually inject them or use the " +
            "<a href=\"{0}\">builder.Services.AddJJMasterData(IConfiguration configuration)</a> overload.",
            "https://portal.jjconsulting.com.br/jjdoc/" +
            "lib/JJMasterData.Web.Extensions.ServiceCollectionExtensions.html" +
            "#JJMasterData_Web_Extensions_ServiceCollectionExtensions_AddJJMasterDataWeb_IServiceCollection_IConfiguration"]);
        return message.ToString();
    }

    public async Task<ConnectionResult> GetConnectionResultAsync(
        string? connectionString, 
        DataAccessProvider provider, 
        CancellationToken cancellationToken = default)
    {
        if (connectionString == null)
            return new ConnectionResult(false, "Connection String not found");

        var dataAccess = new DataAccess(connectionString, provider);
        var result = await dataAccess.TryConnectionAsync(cancellationToken);
        return new ConnectionResult(result.Item1, result.Item2);
    }
}