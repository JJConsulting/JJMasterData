using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.Test.DataManager.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

public class FieldValidationServiceTests
{
    [Fact]
    public async Task ValidateFields_NullFormValues_ThrowsArgumentNullException()
    {
        // Arrange
        var expressionsServiceMock = new Mock<ExpressionsService>();
        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var service = new FieldValidationService(expressionsServiceMock.Object, Enumerable.Empty<JJMasterData.Core.DataManager.Services.Abstractions.IRuleExecutor>(), localizerMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ValidateFieldsAsync(null!, new Dictionary<string, object?>(), new PageState(), true).AsTask());
    }

    [Fact]
    public async Task ValidateFields_InvalidField_ReturnsError()
    {
        // Arrange
        var expressionsServiceMock = new Mock<ExpressionsService>();
        expressionsServiceMock.Setup(e => e.GetBoolValue(It.IsAny<string>(), It.IsAny<FormStateData>()))
                             .Returns(false);

        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var service = new FieldValidationService(expressionsServiceMock.Object, Enumerable.Empty<JJMasterData.Core.DataManager.Services.Abstractions.IRuleExecutor>(), localizerMock.Object);

        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var formValues = new Dictionary<string, object?>();
        const PageState pageState = new();

        // Act
        var result = await service.ValidateFieldsAsync(formElement, formValues, pageState, true);

        // Assert
        Assert.Empty(result);
    }
    

    [Fact]
    public void ValidateField_RequiredFieldEmpty_ReturnsError()
    {
        // Arrange
        var expressionsServiceMock = new Mock<ExpressionsService>();
        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        localizerMock.Setup(l => l["{0} field is required", It.IsAny<string>()]).Returns(new LocalizedString("Field is required","Field is required"));

        var service = new FieldValidationService(expressionsServiceMock.Object, Enumerable.Empty<JJMasterData.Core.DataManager.Services.Abstractions.IRuleExecutor>(), localizerMock.Object);

        var field = new FormElementField { IsRequired = true, Label = "Field" };
        const string fieldId = "fieldId";
        const string value = "";

        // Act
        var result = service.ValidateField(field, fieldId, value);

        // Assert
        Assert.Equal("Field is required", result);
    }

    [Fact]
    public async Task ValidateFieldsAsync_ImportPageState_ExecutesRulesConfiguredForBeforeImport()
    {
        var expressionsServiceMock = new Mock<ExpressionsService>();
        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var executor = new ImportRuleExecutor();
        var service = new FieldValidationService(expressionsServiceMock.Object, [executor], localizerMock.Object);

        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName",
            Rules =
            [
                new FormElementRule
                {
                    Name = "ImportRule",
                    Language = RuleLanguage.Sql,
                    RunOnBeforeImport = true,
                    Script = "select 'error'"
                }
            ]
        };

        var result = await service.ValidateFieldsAsync(formElement, new Dictionary<string, object?>(), PageState.Import, false);

        Assert.Single(result);
        Assert.Equal("Import validation error", result["rule:import"]);
    }

    private sealed class ImportRuleExecutor : IRuleExecutor
    {
        public RuleLanguage Language => RuleLanguage.Sql;

        public Task<Dictionary<string, string>> ExecuteAsync(
            FormElement formElement,
            FormElementRule rule,
            Dictionary<string, object?> values)
        {
            return Task.FromResult(new Dictionary<string, string>
            {
                ["rule:import"] = "Import validation error"
            });
        }
    }
}
