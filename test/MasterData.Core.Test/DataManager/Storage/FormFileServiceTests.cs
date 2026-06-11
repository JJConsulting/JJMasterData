using System.Text;
using JJMasterData.Commons.Resources;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Storage;

public class FormFileServiceTests
{
    [Fact]
    public async Task GetFilesAsync_WithPersistedAndDraftFiles_ReturnsMergedFiles()
    {
        var fileStorage = new DiskFileStorage();
        var draftId = Guid.NewGuid();
        var folderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var service = new FormFileService(
            new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
            fileStorage,
            Mock.Of<IStringLocalizer<MasterDataResources>>());

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

            Assert.Equal(["new-file.txt", "old-file.txt"], allFiles.Select(file => file.FileName).Order());
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
}
