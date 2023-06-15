using System.Text;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Options;
using JJMasterData.Commons.Options.Abstractions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Web.Areas.DataDictionary.Models;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

namespace JJMasterData.Web.Services;

public class OptionsService : BaseService
{
    private IWritableOptions<ConnectionStrings>? ConnectionStringsWritableOptions { get; }
    private IWritableOptions<ConnectionProviders>? ConnectionProvidersWritableOptions { get; }
    internal IWritableOptions<JJMasterDataCommonsOptions>? JJMasterDataWritableOptions { get; }

    public OptionsService(IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IWritableOptions<ConnectionStrings>? connectionStringsWritableOptions = null,
        IWritableOptions<JJMasterDataCommonsOptions>? masterDataWritableOptions = null,
        IWritableOptions<ConnectionProviders>? connectionProvidersWritableOptions = null)
        : base(validationDictionary, dataDictionaryRepository)
    {
        JJMasterDataWritableOptions = masterDataWritableOptions;
        ConnectionStringsWritableOptions = connectionStringsWritableOptions;
        ConnectionProvidersWritableOptions = connectionProvidersWritableOptions;
    }


    public async Task<(bool, string)> TryConnectionAsync(string? connectionString)
    {
        var dataAccess = new DataAccess(connectionString,
            Enum.Parse<DataAccessProvider>(ConnectionProvidersWritableOptions!.Value.ConnectionString!));

        return await dataAccess.TryConnectionAsync(default);
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
        string? connection = ConnectionStringsWritableOptions?.Value.ConnectionString;
        var connectionResult = await GetConnectionResultAsync(connection);
        var viewModel = new OptionsViewModel
        {
            ConnectionString = new ConnectionString(connection),
            ConnectionProvider = DataAccessProvider.SqlServer,
            FilePath = JJMasterDataWritableOptions?.FilePath,
            IsFullscreen = isFullscreen,
            IsConnectionSuccessful = connectionResult.IsConnectionSuccessful
        };

        if (!viewModel.PathExists)
            AddError(nameof(OptionsViewModel.FilePath),
                Translate.Key("{0} does not exists.", viewModel.FilePath ?? "File path"));

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

    private static string GetWritableOptionsErrorMessage()
    {
        var message = new StringBuilder();
        message.AppendLine(Translate.Key(
            "You cannot save your options because they don't use <a href=\"{0}\">IWritableOptions.</a>",
            "https://portal.jjconsulting.com.br/jjdoc/" +
            "lib/JJMasterData.Web.Models.Abstractions.IWritableOptions.html"));
        message.AppendLine(Translate.Key(
            "You can manually inject them or use the " +
            "<a href=\"{0}\">builder.Services.AddJJMasterData(IConfiguration configuration)</a> overload.",
            "https://portal.jjconsulting.com.br/jjdoc/" +
            "lib/JJMasterData.Web.Extensions.ServiceCollectionExtensions.html" +
            "#JJMasterData_Web_Extensions_ServiceCollectionExtensions_AddJJMasterDataWeb_IServiceCollection_IConfiguration"));
        return message.ToString();
    }

    public async Task<ConnectionResult> GetConnectionResultAsync(string? connectionString)
    {
        var result = await TryConnectionAsync(connectionString);
        return new ConnectionResult(result.Item1, result.Item2);
    }
}