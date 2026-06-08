using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Testing;

namespace JJMasterData.IntegrationTests.Shared;

public sealed class MasterDataWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    private readonly string _connectionString;
    private readonly string _exportationFolderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata", "integration-tests", "exports");

    public MasterDataWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
        UseKestrel(0);

        Environment.SetEnvironmentVariable("JJMasterData:ConnectionString", connectionString);
        Environment.SetEnvironmentVariable("JJMasterData:ExportationFolderPath", _exportationFolderPath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JJMasterData:ConnectionString"] = _connectionString,
                ["JJMasterData:ExportationFolderPath"] = _exportationFolderPath
            });
        });

        builder.ConfigureServices(services =>
        {
            services.PostConfigure<HttpsRedirectionOptions>(options =>
            {
                options.HttpsPort = null;
            });
        });
    }
}
