using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.Test.DataManager.Services;

using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class FormServiceTests
{
    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        // Arrange
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var values = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return no errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, PageState.Update, false))
            .ReturnsAsync(new Dictionary<string, string>());

        // Act
        var result = await formService.UpdateAsync(formElement, values, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task UpdateAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        // Arrange
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var values = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return validation errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, PageState.Update, false))
            .ReturnsAsync(new Dictionary<string, string> { { "Field1", "Validation Error" } });

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
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var values = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return no errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, PageState.Insert, false))
            .ReturnsAsync(new Dictionary<string, string>());

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
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var values = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return validation errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, PageState.Insert, false))
            .ReturnsAsync(new Dictionary<string, string> { { "Field1", "Validation Error" } });

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
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var values = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return no errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, PageState.Import, false))
            .ReturnsAsync(new Dictionary<string, string>());

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
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var values = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return validation errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, values, PageState.Import, false))
            .ReturnsAsync(new Dictionary<string, string> { { "Field1", "Validation Error" } });

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
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var primaryKeys = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return no errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, primaryKeys, PageState.Delete, false))
            .ReturnsAsync(new Dictionary<string, string>());

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
        var expressionsServiceMock = new Mock<IExpressionsService>();
        var formFileServiceMock = new Mock<IFormFileService>();
        var fieldValidationServiceMock = new Mock<IFieldValidationService>();
        var auditLogServiceMock = new Mock<IAuditLogService>();

        var formService = new FormService(
            entityRepositoryMock.Object,
            expressionsServiceMock.Object,
            formFileServiceMock.Object,
            fieldValidationServiceMock.Object,
            auditLogServiceMock.Object);

        var formElement = new FormElement();
        var primaryKeys = new Dictionary<string, object>();
        var dataContext = new DataContext();

        // Mock the FieldValidationService to return validation errors
        fieldValidationServiceMock.Setup(fvs => fvs.ValidateFieldsAsync(formElement, primaryKeys, PageState.Delete, false))
            .ReturnsAsync(new Dictionary<string, string> { { "Field1", "Validation Error" } });

        // Act
        var result = await formService.DeleteAsync(formElement, primaryKeys, dataContext);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, result.NumberOfRowsAffected);
    }
}