using System.Diagnostics;
using System.Runtime.InteropServices;
using DotNet.Testcontainers.Images;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.MsSql;

namespace JJMasterData.IntegrationTests.Shared;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private const string LinuxChromiumExecutablePath = "/usr/bin/chromium-browser";
    private MsSqlContainer _container = null!;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public HttpClient Client { get; private set; } = null!;
    public NullLoggerFactory LoggerFactory { get; private set; } = null!;
    public MasterDataWebApplicationFactory<Program> WebFactory { get; private set; } = null!;
    public Uri WebRootUri { get; private set; } = null!;
    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        MsSqlBuilder builder;

        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            builder = new MsSqlBuilder(new DockerImage("mcr.microsoft.com/azure-sql-edge", new Platform("linux/arm64")));
        }
        else
        {
            builder = new MsSqlBuilder("mcr.microsoft.com/azure-sql-edge");
        }

        var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        if (!string.IsNullOrWhiteSpace(dockerHost))
        {
            builder = builder.WithDockerEndpoint(dockerHost);
        }

        _container = builder.Build();
        await _container.StartAsync();

        WebFactory = new MasterDataWebApplicationFactory<Program>(ConnectionString);
        using var webClient = WebFactory.CreateDefaultClient();
        WebRootUri = ResolveRootUri(WebFactory, webClient);

        Client = WebFactory.CreateDefaultClient();
        Client.BaseAddress = new Uri(WebRootUri, "api/");
        LoggerFactory = new NullLoggerFactory();
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();

        if (WebFactory is not null)
        {
            await WebFactory.DisposeAsync();
        }

        Client?.Dispose();
        LoggerFactory?.Dispose();
        await _container.DisposeAsync();
    }

    public async Task<IBrowser> GetBrowserAsync()
    {
        if (_browser is not null)
        {
            return _browser;
        }

        _playwright ??= await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = !Debugger.IsAttached
        };

        if (OperatingSystem.IsLinux() && File.Exists(LinuxChromiumExecutablePath))
        {
            launchOptions.ExecutablePath = LinuxChromiumExecutablePath;
        }

        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
        return _browser;
    }

    public async Task<FormElement?> CreateDataDictionaryAsync(string elementName)
    {
        await using var scope = WebFactory.Services.CreateAsyncScope();
        var dataDictionaryRepository = scope.ServiceProvider.GetRequiredService<IDataDictionaryRepository>();
        var formElement = new FormElement
        {
            Name = elementName,
            TableName = elementName
        };

        await dataDictionaryRepository.InsertOrReplaceAsync(formElement);
        return formElement;
    }

    private static Uri ResolveRootUri(MasterDataWebApplicationFactory<Program> factory, HttpClient client)
    {
        var server = factory.Services.GetRequiredService<IServer>();
        var serverAddresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
        var serverAddress = serverAddresses?.SingleOrDefault(static address => !address.EndsWith(":0", StringComparison.Ordinal));

        return serverAddress is not null
            ? new Uri(serverAddress)
            : client.BaseAddress ?? throw new InvalidOperationException("The test server base address was not initialized.");
    }
}
