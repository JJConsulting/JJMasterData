using System.Reflection;
using System.Text.Json;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Settings;
using JJMasterData.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Web.Services;

public class SettingsService
{
    public JJMasterDataOptions Options { get; }

    public SettingsService(IWritableOptions<JJMasterDataOptions> s,IOptions<JJMasterDataOptions> o, IConfiguration c)
    {
        s.Update(sett =>
        {
            sett.BootstrapVersion = 3;
        });
        
        Options = o.Value;

        var a = JsonConvert.SerializeObject(c);
        var b = a;
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
}