using JJMasterData.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Xunit.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddJJMasterDataCore();
    }
}