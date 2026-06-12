using System.Text;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Storage;

public class FormFileServiceTests
{
    [Fact]
    public async Task CreateFileAsync_WithBlockedExtension_ReturnsValidationError()
    {
        var fileStorage = new DiskFileStorage();
        var draftFolderPath = GetDraftFolderPath(Guid.NewGuid());
        var folderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var stringLocalizer = CreateStringLocalizer();
        var elementFileService = new ElementFileService(
            Mock.Of<IDataDictionaryRepository>(),
            Mock.Of<IEntityRepository>(),
            fileStorage,
            new FileValidationService(stringLocalizer));
        var manager = new UploadViewManager(elementFileService, fileStorage, stringLocalizer);

        var result = await manager.CreateFileAsync(
            draftFolderPath,
            folderPath,
            autoSave: false,
            CreateFormFile("virus.exe", "bad"),
            isMultipleFiles: true);

        Assert.False(result.IsSuccess);
        Assert.Equal("You cannot upload system files", result.ErrorMessage);
    }

    [Fact]
    public async Task GetFilesAsync_WithPersistedAndDraftFiles_ReturnsMergedFiles()
    {
        var fileStorage = new DiskFileStorage();
        var draftId = Guid.NewGuid();
        var draftFolderPath = GetDraftFolderPath(draftId);
        var folderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var stringLocalizer = Mock.Of<IStringLocalizer<MasterDataResources>>();
        var elementFileService = new ElementFileService(
            Mock.Of<IDataDictionaryRepository>(),
            Mock.Of<IEntityRepository>(),
            fileStorage,
            new FileValidationService(stringLocalizer));
        var manager = new UploadViewManager(elementFileService, fileStorage, stringLocalizer);

        try
        {
            var fullPath = FileStoragePath.Combine(folderPath, "old-file.txt");
            await fileStorage.SaveAsync(fullPath, CreateStream("old"), true, TestContext.Current.CancellationToken);
            await manager.CreateFileAsync(
                draftFolderPath,
                folderPath,
                autoSave: false,
                CreateFormFile("new-file.txt", "new"),
                isMultipleFiles: true);

            var allFiles = await manager.GetFilesAsync(draftFolderPath, folderPath);

            Assert.Equal(["new-file.txt", "old-file.txt"], allFiles.Select(file => file.FileName).Order());
        }
        finally
        {
            await fileStorage.DeleteFolderAsync(folderPath, TestContext.Current.CancellationToken);
            await fileStorage.DeleteFolderAsync(draftFolderPath, TestContext.Current.CancellationToken);
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

    private static IStringLocalizer<MasterDataResources> CreateStringLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<MasterDataResources>>();
        localizer
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        localizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        return localizer.Object;
    }

    private static string GetDraftFolderPath(Guid draftId) =>
        "{app.path}/MasterDataDraftFiles/" + draftId.ToString("N") + "/";
}
