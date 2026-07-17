using System.Text;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;

namespace JJMasterData.Core.Test.UI.Components.IO.UploadView;

public class JJUploadViewTests
{
    [Fact]
    public async Task GetFilesAsync_WhenSingleFileAndDraftFilesExist_ReturnsOnlyDraftFiles()
    {
        var fileStorage = new DiskFileStorage();
        var contextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var draftId = Guid.NewGuid();
        contextAccessor.HttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["upload-view-draft-id"] = draftId.ToString("N")
        });
        var folderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var manager = CreateUploadViewManager(contextAccessor, fileStorage);
        var uploadView = CreateUploadView(contextAccessor, manager);
        uploadView.FolderPath = folderPath;
        uploadView.AutoSave = false;
        uploadView.UploadArea.Multiple = false;

        try
        {
            var fullPath = FileStoragePath.Combine(folderPath, "old-file.txt");
            await fileStorage.SaveAsync(fullPath, CreateStream("old"), true, TestContext.Current.CancellationToken);
            await uploadView.CreateFileAsync(CreateFormFile("new-file.txt", "new"));

            var files = await uploadView.GetFilesAsync();

            Assert.Equal(["new-file.txt"], files.Select(file => file.FileName));
            Assert.All(files, file => Assert.Equal(uploadView.TempPath, file.FolderPath));
        }
        finally
        {
            await fileStorage.DeleteFolderAsync(folderPath, TestContext.Current.CancellationToken);
            await fileStorage.DeleteFolderAsync(uploadView.TempPath, TestContext.Current.CancellationToken);
        }
    }

    private static MemoryStream CreateStream(string value)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }

    private static FormFile CreateFormFile(string fileName, string value)
    {
        var stream = CreateStream(value);
        return new FormFile(stream, 0, stream.Length, "file", fileName);
    }

    private static UploadViewManager CreateUploadViewManager(
        IHttpContextAccessor contextAccessor,
        IFileStorage fileStorage)
    {
        var stringLocalizer = Mock.Of<IStringLocalizer<MasterDataResources>>();
        var elementFileService = new ElementFileService(
            Mock.Of<IDataDictionaryRepository>(),
            Mock.Of<IEntityRepository>(),
            fileStorage,
            new FileValidationService(stringLocalizer));

        return new UploadViewManager(contextAccessor, elementFileService, fileStorage, stringLocalizer);
    }

    private static JJUploadView CreateUploadView(
        IHttpContextAccessor contextAccessor,
        UploadViewManager manager)
    {
        var stringLocalizer = Mock.Of<IStringLocalizer<MasterDataResources>>();
        var encryptionService = new Mock<IEncryptionService>();
        encryptionService
            .Setup(service => service.EncryptString(It.IsAny<string>()))
            .Returns((string value, string _) => value);

        var uploadAreaFactory = new UploadAreaFactory(
            contextAccessor,
            new UploadAreaManager(contextAccessor, new FileValidationService(stringLocalizer)),
            encryptionService.Object,
            Options.Create(new FormOptions()),
            stringLocalizer);

        var componentFactory = new Mock<IComponentFactory>();
        componentFactory
            .SetupGet(factory => factory.UploadArea)
            .Returns(uploadAreaFactory);

        return new JJUploadView(
            contextAccessor,
            componentFactory.Object,
            manager,
            encryptionService.Object,
            stringLocalizer,
            NullLoggerFactory.Instance);
    }
}
