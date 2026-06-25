using System.Linq;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JJMasterData.Core.Test.Configuration;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddJJMasterDataCore_WhenCalledMoreThanOnce_DoesNotDuplicateRuleExecutors()
    {
        var services = new ServiceCollection();

        services.AddJJMasterDataCore();
        services.AddJJMasterDataCore();

        var ruleExecutors = services
            .Where(service => service.ServiceType == typeof(IRuleExecutor))
            .Select(service => service.ImplementationType)
            .ToArray();

        Assert.Equal(2, ruleExecutors.Length);
        Assert.Contains(typeof(SqlRuleExecutor), ruleExecutors);
        Assert.Contains(typeof(JavaScriptRuleScriptExecutor), ruleExecutors);
    }
}
