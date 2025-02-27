using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.Test.DataManager.Services;

using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

public class FieldValidationServiceTests
{
    [Fact]
    public void ValidateFields_NullFormValues_ThrowsArgumentNullException()
    {
        // Arrange
        var expressionsServiceMock = new Mock<ExpressionsService>();
        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var service = new FieldValidationService(expressionsServiceMock.Object, localizerMock.Object);

        // Act and Assert
        Assert.Throws<ArgumentNullException>(() => service.ValidateFields(null, new Dictionary<string, object>(), new PageState(), true));
    }

    [Fact]
    public void ValidateFields_InvalidField_ReturnsError()
    {
        // Arrange
        var expressionsServiceMock = new Mock<ExpressionsService>();
        expressionsServiceMock.Setup(e => e.GetBoolValue(It.IsAny<string>(), It.IsAny<FormStateData>()))
                             .Returns(false);

        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        var service = new FieldValidationService(expressionsServiceMock.Object, localizerMock.Object);

        var formElement = new FormElement
        {
            Name = "name",
            TableName = "tableName"
        };
        var formValues = new Dictionary<string, object>();
        const PageState pageState = new();

        // Act
        var result = service.ValidateFields(formElement, formValues, pageState, true);

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

        var service = new FieldValidationService(expressionsServiceMock.Object, localizerMock.Object);

        var field = new FormElementField { IsRequired = true, Label = "Field" };
        const string fieldId = "fieldId";
        const string value = "";

        // Act
        var result = service.ValidateField(field, fieldId, value);

        // Assert
        Assert.Equal("Field is required", result);
    }
}
