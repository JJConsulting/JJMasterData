using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.IO;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Test.DataManager.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

public class FormServiceTests
{
    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        // Arrange
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "name"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement,values);


        var dataContext = new DataContext();

        // Mock the FieldValidationService to return no errors


        // Act
        var result = await formService.UpdateAsync(formElement, values, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    private static FormService GetFormService(FormElement formElement, Dictionary<string,object> values)
    {
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var formFileServiceMock = new Mock<FormFileService>();
        var fieldValidationServiceMock = new Mock<FieldValidationService>();
        var stringLocalizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var auditLogServiceMock = new Mock<AuditLogService>();
        var loggerMock = new Mock<ILogger<FormService>>();
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFields(formElement, values, PageState.Update, false))
            .Returns(new Dictionary<string, string>());

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
        // Arrange
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement,values);
        var dataContext = new DataContext();
        
        // Act
        var result = await formService.UpdateAsync(formElement, values, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task InsertAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        // Arrange
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement,values);
        var dataContext = new DataContext();

        // Act
        var result = await formService.InsertAsync(formElement, values, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task InsertAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        // Arrange
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement,values);
        var dataContext = new DataContext();
        
        // Act
        var result = await formService.InsertAsync(formElement, values, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task InsertOrReplaceAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        // Arrange
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement,values);
        var dataContext = new DataContext();

        // Act
        var result = await formService.InsertOrReplaceAsync(formElement, values, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task InsertOrReplaceAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        // Arrange
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var values = new Dictionary<string, object>();
        var formService = GetFormService(formElement,values);
        var dataContext = new DataContext();

        // Act
        var result = await formService.InsertOrReplaceAsync(formElement, values, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        // Arrange
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var formFileServiceMock = new Mock<FormFileService>();
        var fieldValidationServiceMock = new Mock<FieldValidationService>();
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
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return no errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFields(formElement, primaryKeys, PageState.Delete, false))
            .Returns(new Dictionary<string, string>());

        // Mock EntityRepository to return a positive number of rows affected
        entityRepositoryMock.Setup(er => er.DeleteAsync(formElement, primaryKeys)).ReturnsAsync(1);

        // Act
        var result = await formService.DeleteAsync(formElement, primaryKeys, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal(1, result.NumberOfRowsAffected);
    }

    [Fact]
    public async Task DeleteAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        // Arrange
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var stringLocalizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var formFileServiceMock = new Mock<FormFileService>();
        var fieldValidationServiceMock = new Mock<FieldValidationService>();
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
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return validation errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFields(formElement, primaryKeys, PageState.Delete, false))
            .Returns(new Dictionary<string, string> { { "Field1", "Validation Error" } });

        // Act
        var result = await formService.DeleteAsync(formElement, primaryKeys, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, result.NumberOfRowsAffected);
    }
}