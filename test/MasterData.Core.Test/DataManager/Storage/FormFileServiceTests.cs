using System.Text;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Storage;

public class FormFileServiceTests
{
    [Fact]
    public async Task GetFilesAsync_WhenPreferTemporaryFiles_ReturnsOnlyTemporaryFiles()
    {
        var fileStorage = new DiskFileStorage();
        var temporaryUploadStore = new TemporaryDiskFileStore();
        var draftId = Guid.NewGuid().ToString("N");
        var folderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var service = new FormFileService(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            temporaryUploadStore,
            fileStorage,
            Mock.Of<IStringLocalizer<MasterDataResources>>(),
            NullLoggerFactory.Instance);

        try
        {
            var fullPath = FileStoragePath.Combine(folderPath, "old-file.txt");
            await fileStorage.SaveAsync(fullPath, CreateStream("old"), true, TestContext.Current.CancellationToken);
            await service.CreateFileAsync(
                draftId,
                folderPath,
                autoSave: false,
                new FormFileContent
                {
                    FileName = "new-file.txt",
                    Stream = CreateStream("new")
                },
                replaceIfExists: true);

            var allFiles = await service.GetFilesAsync(draftId, folderPath);
            var preferredFiles = await service.GetFilesAsync(draftId, folderPath, preferTemporaryFiles: true);

            Assert.Equal(["new-file.txt", "old-file.txt"], allFiles.Select(file => file.FileName).Order());
            Assert.Equal(["new-file.txt"], preferredFiles.Select(file => file.FileName));
            Assert.All(preferredFiles, file => Assert.True(file.IsTemporary));
        }
        finally
        {
            await fileStorage.DeleteFolderAsync(folderPath, TestContext.Current.CancellationToken);
            await temporaryUploadStore.DeleteFolderAsync(draftId, TestContext.Current.CancellationToken);
        }
    }

    private static MemoryStream CreateStream(string value)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }
}
