using System.Reflection;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Settings;
using JJMasterData.Web.Areas.MasterData.Models;
using JJMasterData.Web.Models.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Web.Services;

public class SettingsService
{
    private IOptionsWriter<ConnectionStrings> ConnectionStringsOptionsWriter { get; }
    private IOptionsWriter<JJMasterDataOptions> JJMasterDataOptionsWriter { get; }
    public JJMasterDataOptions Options { get; }
    
    public SettingsService(IOptionsWriter<JJMasterDataOptions> masterDataOptionsWriter,IOptionsSnapshot<JJMasterDataOptions> options, IOptionsWriter<ConnectionStrings> connectionStringsOptionsWriter)
    {
        JJMasterDataOptionsWriter = masterDataOptionsWriter;
        ConnectionStringsOptionsWriter = connectionStringsOptionsWriter;
        Options = options.Value;
    }

    public List<Assembly> GetJJAssemblies() => AppDomain.CurrentDomain.GetAssemblies().Where(a =>
    {
        var name = a.GetName().Name;
        return name != null && name.Contains("JJ");
    }).ToList();

    public string GetAssemblyCopyright(Assembly assembly)
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
    }

    public string GetAssemblyProduct(Assembly assembly)
    {
        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        return attributes.Length == 0 ? string.Empty : ((AssemblyProductAttribute)attributes[0]).Product;
    }
    public async Task<(bool, string)> TryConnectionAsync(string connectionString)
    {
        var dataAccess = new DataAccess
        {
            ConnectionString = connectionString
        };

        return await dataAccess.TryConnectionAsync();
    }

    public async Task SaveOptions(SettingsViewModel model)
    {
        await ConnectionStringsOptionsWriter.UpdateAsync(options =>
        {
            options.ConnectionString = model.ConnectionString.ToString();
        });
        await JJMasterDataOptionsWriter.UpdateAsync(options =>
        {
            options.BootstrapVersion = model.BootstrapVersion;
        });
        
    }
}