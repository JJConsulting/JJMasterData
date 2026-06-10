using System.Text;
using JJMasterData.Commons.Resources;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace JJMasterData.Core.Test.UI.Components.IO.UploadView;

public class JJUploadViewTests
{
    [Fact]
    public async Task GetFilesAsync_WhenSingleFileAndDraftFilesExist_ReturnsOnlyDraftFiles()
    {
        var fileStorage = new DiskFileStorage();
        var contextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var folderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var formFileService = new FormFileService(
            contextAccessor,
            fileStorage,
            Mock.Of<IStringLocalizer<MasterDataResources>>());
        var uploadView = CreateUploadView(contextAccessor, formFileService);
        uploadView.FolderPath = folderPath;
        uploadView.AutoSave = false;
        uploadView.UploadArea.Multiple = false;
        var draftId = uploadView.DraftId;

        try
        {
            var fullPath = FileStoragePath.Combine(folderPath, "old-file.txt");
            await fileStorage.SaveAsync(fullPath, CreateStream("old"), true, TestContext.Current.CancellationToken);
            await uploadView.CreateFileAsync(new FormFileContent
            {
                FileName = "new-file.txt",
                Stream = CreateStream("new")
            });

            var files = await uploadView.GetFilesAsync();

            Assert.Equal(["new-file.txt"], files.Select(file => file.FileName));
            Assert.All(files, file => Assert.Equal(FormFileService.GetDraftFolderPath(draftId), file.FolderPath));
        }
        finally
        {
            await fileStorage.DeleteFolderAsync(folderPath, TestContext.Current.CancellationToken);
            await fileStorage.DeleteFolderAsync(FormFileService.GetDraftFolderPath(draftId), TestContext.Current.CancellationToken);
        }
    }

    private static MemoryStream CreateStream(string value)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }

    private static JJUploadView CreateUploadView(
        IHttpContextAccessor contextAccessor,
        FormFileService formFileService)
    {
        var stringLocalizer = Mock.Of<IStringLocalizer<MasterDataResources>>();
        var uploadAreaFactory = new UploadAreaFactory(
            contextAccessor,
            new UploadAreaService(contextAccessor, stringLocalizer),
            Mock.Of<IEncryptionService>(),
            Options.Create(new FormOptions()),
            stringLocalizer);

        var componentFactory = new Mock<IComponentFactory>();
        componentFactory
            .SetupGet(factory => factory.UploadArea)
            .Returns(uploadAreaFactory);

        return new JJUploadView(
            contextAccessor,
            componentFactory.Object,
            formFileService,
            Mock.Of<IEncryptionService>(),
            stringLocalizer,
            NullLoggerFactory.Instance);
    }
}
