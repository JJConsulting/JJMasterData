using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Providers;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Configuration.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

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
        var values = new Dictionary<string, object?>();
        var formService = GetFormService();

        var result = await formService.UpdateAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.Empty(result.Errors);
    }

    private static FormService GetFormService(params IRuleExecutor[] ruleExecutors)
    {
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var stringLocalizer = CreateStringLocalizer();
        var fieldValidationService = new FieldValidationService(
            CreateExpressionsService(),
            ruleExecutors,
            stringLocalizer);
        var auditLogService = new AuditLogService(
            entityRepositoryMock.Object,
            Mock.Of<IOptionsSnapshot<MasterDataCoreOptions>>());

        return new FormService(
            entityRepositoryMock.Object,
            fieldValidationService,
            auditLogService,
            stringLocalizer,
            Mock.Of<ILogger<FormService>>());
    }

    [Fact]
    public async Task UpdateAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        AddRequiredField(formElement);
        var values = new Dictionary<string, object?>();
        var formService = GetFormService();

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
        var values = new Dictionary<string, object?>();
        var formService = GetFormService();

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
        AddRequiredField(formElement);
        var values = new Dictionary<string, object?>();
        var formService = GetFormService();

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
        var values = new Dictionary<string, object?>();
        var formService = GetFormService();

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
        AddRequiredField(formElement);
        var values = new Dictionary<string, object?>();
        var formService = GetFormService();

        var result = await formService.InsertOrReplaceAsync(formElement, values, new DataContext());

        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task DeleteAsync_WithValidData_ReturnsFormLetterWithNoErrors()
    {
        var entityRepositoryMock = new Mock<IEntityRepository>();
        var stringLocalizer = CreateStringLocalizer();
        var fieldValidationService = new FieldValidationService(
            CreateExpressionsService(),
            [],
            stringLocalizer);
        var auditLogService = new AuditLogService(
            entityRepositoryMock.Object,
            Mock.Of<IOptionsSnapshot<MasterDataCoreOptions>>());
        var formService = new FormService(
            entityRepositoryMock.Object,
            fieldValidationService,
            auditLogService,
            stringLocalizer,
            Mock.Of<ILogger<FormService>>());

        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var primaryKeys = new Dictionary<string, object>();

        entityRepositoryMock.Setup(er => er.DeleteAsync(formElement, primaryKeys)).ReturnsAsync(1);

        var result = await formService.DeleteAsync(formElement, primaryKeys, new DataContext());

        Assert.NotNull(result);
        Assert.Empty(result.Errors);
        Assert.Equal(1, result.NumberOfRowsAffected);
    }

    [Fact]
    public async Task DeleteAsync_WithValidationErrors_ReturnsFormLetterWithErrors()
    {
        var formElement = new FormElement
        {
            Name = "name",
            TableName = "name",
            Rules =
            [
                new FormElementRule
                {
                    Name = "DeleteRule",
                    Language = RuleLanguage.Sql,
                    RunOnBeforeDelete = true
                }
            ]
        };
        var primaryKeys = new Dictionary<string, object>();
        var formService = GetFormService(
            new TestRuleExecutor(new Dictionary<string, string>
            {
                ["Field1"] = "Validation Error"
            }));

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
        var values = new Dictionary<string, object?>();

        var entityRepositoryMock = new Mock<IEntityRepository>();
        formElement.Rules.Add(new FormElementRule
        {
            Name = "InsertRule",
            Language = RuleLanguage.Sql,
            RunOnBeforeInsert = true
        });
        var stringLocalizer = CreateStringLocalizer();
        var fieldValidationService = new FieldValidationService(
            CreateExpressionsService(),
            [
                new TestRuleExecutor(new Dictionary<string, string>
                {
                    ["validation:test"] = "Script error"
                })
            ],
            stringLocalizer);
        var formService = new FormService(
            entityRepositoryMock.Object,
            fieldValidationService,
            new AuditLogService(
                entityRepositoryMock.Object,
                Mock.Of<IOptionsSnapshot<MasterDataCoreOptions>>()),
            stringLocalizer,
            Mock.Of<ILogger<FormService>>());

        var result = await formService.InsertAsync(formElement, values, new DataContext());

        Assert.Single(result.Errors);
        entityRepositoryMock.Verify(r => r.InsertAsync(It.IsAny<FormElement>(), It.IsAny<Dictionary<string, object?>>()), Times.Never);
    }

    private static void AddRequiredField(FormElement formElement)
    {
        formElement.Fields.Add(new FormElementField
        {
            Name = "RequiredField",
            Label = "Required field",
            IsRequired = true
        });
    }

    private static ExpressionsService CreateExpressionsService()
    {
        IExpressionProvider[] providers = [new ValueExpressionProvider()];
        var expressionParser = new ExpressionParser(
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IMasterDataUser>(),
            Mock.Of<ILogger<ExpressionParser>>());

        return new ExpressionsService(
            providers,
            expressionParser,
            new DataProtectionService(new EphemeralDataProtectionProvider()),
            Mock.Of<ILogger<ExpressionsService>>());
    }

    private static IStringLocalizer<MasterDataResources> CreateStringLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<MasterDataResources>>();
        localizer
            .Setup(value => value[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        localizer
            .Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) =>
                new LocalizedString(name, string.Format(name, arguments)));
        return localizer.Object;
    }

    private sealed class TestRuleExecutor(Dictionary<string, string> errors) : IRuleExecutor
    {
        public RuleLanguage Language => RuleLanguage.Sql;

        public Task<Dictionary<string, string>> ExecuteAsync(
            FormElement formElement,
            FormElementRule rule,
            Dictionary<string, object?> values)
        {
            return Task.FromResult(errors);
        }
    }
}
