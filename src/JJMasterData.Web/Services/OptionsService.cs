using System.Reflection;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Options;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Models.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Services;

public class OptionsService
{
    private IOptionsWriter<ConnectionStrings> ConnectionStringsOptionsWriter { get; }
    private IOptionsWriter<JJMasterDataOptions> JJMasterDataOptionsWriter { get; }
    public JJMasterDataOptions Options { get; }

    public OptionsService(IOptionsWriter<JJMasterDataOptions> masterDataOptionsWriter,
        IOptionsSnapshot<JJMasterDataOptions> options, IOptionsWriter<ConnectionStrings> connectionStringsOptionsWriter)
    {
        JJMasterDataOptionsWriter = masterDataOptionsWriter;
        ConnectionStringsOptionsWriter = connectionStringsOptionsWriter;
        Options = options.Value;
    }
    

    public async Task<(bool, string)> TryConnectionAsync(string connectionString)
    {
        var dataAccess = new DataAccess
        {
            ConnectionString = connectionString
        };

        return await dataAccess.TryConnectionAsync();
    }

    public async Task SaveOptions(OptionsViewModel model)
    {
        await ConnectionStringsOptionsWriter.UpdateAsync(options =>
        {
            options.ConnectionString = model.ConnectionString.ToString();
        });

        await JJMasterDataOptionsWriter.UpdateAsync(options =>
        {
            options.BootstrapVersion = model.Options.BootstrapVersion;
            options.Logger = model.Options.Logger;
        });
    }
}