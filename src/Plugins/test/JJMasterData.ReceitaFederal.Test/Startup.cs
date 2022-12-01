using JJMasterData.Commons.Extensions;
using JJMasterData.ReceitaFederal.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.ReceitaFederal.Test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddJJMasterDataCommons().WithSintegra().WithServicesHub();
    }
}