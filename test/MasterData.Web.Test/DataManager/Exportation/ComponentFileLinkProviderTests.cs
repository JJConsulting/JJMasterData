using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Web.Components;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace JJMasterData.Web.Test.DataManager.Exportation;

public class ComponentFileLinkProviderTests
{
    [Fact]
    public void GetFileUrl_ShouldUseCurrentComponentUrl()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.test");
        context.Request.PathBase = "/app";
        context.Request.Path = "/customers";
        context.Request.QueryString = new QueryString("?page=2");

        var contextAccessor = new HttpContextAccessor { HttpContext = context };
        var encryptionService = new Mock<IEncryptionService>();
        encryptionService
            .Setup(service => service.EncryptString(It.IsAny<string>()))
            .Returns((string value) => $"encrypted-{value.Length}");
        var downloaderFactory = new FileDownloaderFactory(
            contextAccessor,
            Mock.Of<IFileStorage>(),
            encryptionService.Object);
        var provider = new ComponentFileLinkProvider(downloaderFactory, contextAccessor);
        var formElement = CreateFormElement();

        var link = provider.GetFileUrl(
            formElement,
            formElement.Fields["Document"],
            new Dictionary<string, object?> { ["Id"] = 10 },
            "invoice.pdf");

        var uri = new Uri(link!);
        Assert.Equal("https://example.test/app/customers", uri.GetLeftPart(UriPartial.Path));
        Assert.Contains("page=2", uri.Query);
        Assert.Contains("routeContext=", uri.Query);
        Assert.Contains("downloadFileToken=", uri.Query);
    }

    private static FormElement CreateFormElement()
    {
        return new FormElement
        {
            Name = "Customers",
            Fields =
            [
                new FormElementField
                {
                    Name = "Id",
                    IsPk = true
                },
                new FormElementField
                {
                    Name = "Document",
                    Component = FormComponent.File,
                    DataFile = new FormElementDataFile
                    {
                        FolderPath = "/customers/files"
                    }
                }
            ]
        };
    }
}
