using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JJMasterData.Core.Test.DataManager.Services;

public class FormServiceTests
{
    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "name"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement, values);

        var result = await formService.UpdateAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    private static FormService GetFormService(FormElement formElement, Dictionary<string, object> values)
    {
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var formFileServiceMock = new Mock<FormFileService>();
        var fieldValidationServiceMock = new Mock<FieldValidationService>(
            Mock.Of<JJMasterData.Core.DataManager.Expressions.ExpressionsService>(),
            Enumerable.Empty<JJMasterData.Core.DataManager.Services.Abstractions.IValidationScriptExecutor>(),
            Mock.Of<IStringLocalizer<MasterDataResources>>());
        var stringLocalizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var auditLogServiceMock = new Mock<AuditLogService>();
        var loggerMock = new Mock<ILogger<FormService>>();

        fieldValidationServiceMock
            .Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, It.IsAny<PageState>(), false, It.IsAny<CommandOperation>()))
            .ReturnsAsync(new Dictionary<string, string>());

        return new FormService(
            entityRepositoryMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object,
            stringLocalizerMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task UpdateAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement, values);

        var result = await formService.UpdateAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task InsertAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement, values);

        var result = await formService.InsertAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task InsertAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement, values);

        var result = await formService.InsertAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task InsertOrReplaceAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement, values);

        var result = await formService.InsertOrReplaceAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task InsertOrReplaceAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement, values);

        var result = await formService.InsertOrReplaceAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var formFileServiceMock = new Mock<FormFileService>();
        var fieldValidationServiceMock = new Mock<FieldValidationService>(
            Mock.Of<JJMasterData.Core.DataManager.Expressions.ExpressionsService>(),
            Enumerable.Empty<JJMasterData.Core.DataManager.Services.Abstractions.IValidationScriptExecutor>(),
            Mock.Of<IStringLocalizer<MasterDataResources>>());
        var auditLogServiceMock = new Mock<AuditLogService>();
        var stringLocalizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var loggerMock = new Mock<ILogger<FormService>>();
        var formService = new FormService(
            entityRepositoryMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object,
            stringLocalizerMock.Object,
            loggerMock.Object);

        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var primaryKeys = new Dictionary<string, object>();

        fieldValidationServiceMock
            .Setup(fvs => fvs.ValidateFieldsAsync(formElement, primaryKeys, PageState.Delete, false, CommandOperation.Delete))
            .ReturnsAsync(new Dictionary<string, string>());

        entityRepositoryMock.Setup(er => er.DeleteAsync(formElement, primaryKeys)).ReturnsAsync(1);

        var result = await formService.DeleteAsync(formElement, primaryKeys, new DataContext());

        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal(1, result.NumberOfRowsAffected);
    }

    [Fact]
    public async Task DeleteAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var stringLocalizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var formFileServiceMock = new Mock<FormFileService>();
        var fieldValidationServiceMock = new Mock<FieldValidationService>(
            Mock.Of<JJMasterData.Core.DataManager.Expressions.ExpressionsService>(),
            Enumerable.Empty<JJMasterData.Core.DataManager.Services.Abstractions.IValidationScriptExecutor>(),
            Mock.Of<IStringLocalizer<MasterDataResources>>());
        var auditLogServiceMock = new Mock<AuditLogService>();
        var loggerMock = new Mock<ILogger<FormService>>();
        var formService = new FormService(
            entityRepositoryMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object,
            stringLocalizerMock.Object,
            loggerMock.Object);

        var formElement = new FormElement
        {
            Name = "name",
            TableName = "name"
        };
        var primaryKeys = new Dictionary<string, object>();

        fieldValidationServiceMock
            .Setup(fvs => fvs.ValidateFieldsAsync(formElement, primaryKeys, PageState.Delete, false, CommandOperation.Delete))
            .ReturnsAsync(new Dictionary<string, string> { { "Field1", "Validation Error" } });

        var result = await formService.DeleteAsync(formElement, primaryKeys, new DataContext());

        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, result.NumberOfRowsAffected);
    }

    [Fact]
    public async Task InsertAsync_WithScriptValidationErrors_ReturnsFormLetterWithErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();

        var entityRepositoryMock = new Mock<IEntityRepository>();
        var formFileServiceMock = new Mock<FormFileService>();
        var fieldValidationServiceMock = new Mock<FieldValidationService>(
            Mock.Of<JJMasterData.Core.DataManager.Expressions.ExpressionsService>(),
            Enumerable.Empty<JJMasterData.Core.DataManager.Services.Abstractions.IValidationScriptExecutor>(),
            Mock.Of<IStringLocalizer<MasterDataResources>>());
        var auditLogServiceMock = new Mock<AuditLogService>();
        var stringLocalizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var loggerMock = new Mock<ILogger<FormService>>();

        fieldValidationServiceMock
            .Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, PageState.Insert, false, CommandOperation.Insert))
            .ReturnsAsync(new Dictionary<string, string> { { "validation:test", "Script error" } });

        var formService = new FormService(
            entityRepositoryMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object,
            stringLocalizerMock.Object,
            loggerMock.Object);

        var result = await formService.InsertAsync(formElement, values, new DataContext());

        Assert.Single(result.Errors);
        entityRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<FormElement>(), It.IsAny<Dictionary<string, object>>()), Times.Never);
    }
}
