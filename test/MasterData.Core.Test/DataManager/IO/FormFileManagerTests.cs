using System.Text;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.IO.Storage;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;

namespace JJMasterData.Core.Test.DataManager.IO;

public class FormFileManagerTests
{
    [Fact]
    public async Task GetFilesAsync_WhenPreferTemporaryFiles_ReturnsOnlyTemporaryFiles()
    {
        var fileStorage = new DiskFileStorage();
        var temporaryUploadStore = new TemporaryDiskUploadStore();
        var draftId = Guid.NewGuid().ToString("N");
        var folderKey = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var manager = new FormFileManager(
            draftId,
            temporaryUploadStore,
            fileStorage,
            Mock.Of<IStringLocalizer<MasterDataResources>>(),
            Mock.Of<ILogger<FormFileManager>>())
        {
            AutoSave = false,
            FolderKey = folderKey
        };

        try
        {
            await fileStorage.SaveAsync(folderKey, "old-file.txt", CreateStream("old"), true);
            await manager.CreateFileAsync(new FormFileContent
            {
                FileName = "new-file.txt",
                Stream = CreateStream("new")
            }, replaceIfExists: true);

            var allFiles = await manager.GetFilesAsync();
            var preferredFiles = await manager.GetFilesAsync(preferTemporaryFiles: true);

            Assert.Equal(["new-file.txt", "old-file.txt"], allFiles.Select(file => file.Content.FileName).Order());
            Assert.Equal(["new-file.txt"], preferredFiles.Select(file => file.Content.FileName));
            Assert.All(preferredFiles, file => Assert.True(file.IsTemporary));
        }
        finally
        {
            await fileStorage.DeleteFolderAsync(folderKey, TestContext.Current.CancellationToken);
            await temporaryUploadStore.DeleteFolderAsync(draftId, TestContext.Current.CancellationToken);
        }
    }

    private static MemoryStream CreateStream(string value)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }
}
