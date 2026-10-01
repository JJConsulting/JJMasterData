using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataManager.Expressions.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NCalc.DependencyInjection;
using NCalc.Factories;

namespace JJMasterData.Core.Test.DataManager.Expressions;

public class DefaultExpressionProviderTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("filled", false)]
    public void QuotedParameterEqualsEmptyStringOnlyWhenEmpty(string? value, bool expected)
    {
        var provider = CreateProvider();

        var result = provider.Evaluate("'{VALOR}' = ''", new Dictionary<string, object?>
        {
            ["VALOR"] = value
        });

        Assert.Equal(expected, result);
    }

    [Fact]
    public void UnquotedNullParameterRemainsNull()
    {
        var provider = CreateProvider();

        var result = provider.Evaluate("{VALOR} = null", new Dictionary<string, object?>
        {
            ["VALOR"] = null
        });

        Assert.Equal(true, result);
    }

    private static DefaultExpressionProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddNCalc();
        var serviceProvider = services.BuildServiceProvider();

        return new DefaultExpressionProvider(
            serviceProvider.GetRequiredService<IExpressionFactory>(),
            serviceProvider,
            Options.Create(new MasterDataCoreOptions()),
            NullLogger<DefaultExpressionProvider>.Instance);
    }
}
