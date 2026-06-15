using System.Text;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using Moq;

namespace JJMasterData.Core.Test.DataManager.Storage;

public class FormFileServiceTests
{
    [Fact]
    public async Task GetFilesAsync_WithPersistedAndDraftFiles_ReturnsMergedFiles()
    {
        var fileStorage = new DiskFileStorage();
        var draftId = Guid.NewGuid();
        var draftFolderPath = GetDraftFolderPath(draftId);
        var folderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"));
        var contextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var stringLocalizer = Mock.Of<IStringLocalizer<MasterDataResources>>();
        var elementFileService = new ElementFileService(
            Mock.Of<IDataDictionaryRepository>(),
            Mock.Of<IEntityRepository>(),
            fileStorage,
            new FileValidationService(stringLocalizer));
        var manager = new UploadViewManager(contextAccessor, elementFileService, fileStorage, stringLocalizer);

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

    [Fact]
    public async Task PromoteDraftFilesAsync_FromFormElement_MovesDraftFilesAndDeletesMarkedFiles()
    {
        var fileStorage = new DiskFileStorage();
        var draftId = Guid.NewGuid();
        var draftFolderPath = GetDraftFolderPath(draftId);
        var formElement = CreateFormElement();
        var values = new Dictionary<string, object> { ["Id"] = "10" };
        var folderPath = FileStoragePathExtensions.GetFolderPath(formElement, formElement.Fields["Document"], values);
        var context = new DefaultHttpContext();
        context.Features.Set<IFormFeature>(new FormFeature(new FormCollection(new Dictionary<string, StringValues>
        {
            ["Document-upload-view-draft-id"] = draftId.ToString("N"),
            ["Document-upload-view-files-deleted"] = "old-file.txt"
        })));
        var contextAccessor = new HttpContextAccessor { HttpContext = context };
        var stringLocalizer = Mock.Of<IStringLocalizer<MasterDataResources>>();
        var elementFileService = new ElementFileService(
            Mock.Of<IDataDictionaryRepository>(),
            Mock.Of<IEntityRepository>(),
            fileStorage,
            new FileValidationService(stringLocalizer));
        var manager = new UploadViewManager(contextAccessor, elementFileService, fileStorage, stringLocalizer);

        try
        {
            await fileStorage.SaveAsync(
                FileStoragePath.Combine(folderPath, "old-file.txt"),
                CreateStream("old"),
                true,
                TestContext.Current.CancellationToken);
            await manager.CreateFileAsync(
                draftFolderPath,
                folderPath,
                autoSave: false,
                CreateFormFile("new-file.txt", "new"),
                isMultipleFiles: true);

            await manager.PromoteDraftFilesAsync(formElement, values);

            var files = await manager.GetFilesAsync(draftFolderPath, folderPath);

            Assert.Equal(["new-file.txt"], files.Select(file => file.FileName));
            Assert.All(files, file => Assert.Equal(folderPath, file.FolderPath));
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

    private static string GetDraftFolderPath(Guid draftId) =>
        "{app.path}/MasterDataDraftFiles/" + draftId.ToString("N") + "/";

    private static FormElement CreateFormElement()
    {
        return new FormElement
        {
            Name = "Test",
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
                        FolderPath = Path.Combine(Path.GetTempPath(), "jjmasterdata-tests", Guid.NewGuid().ToString("N"))
                    }
                }
            ]
        };
    }
}
