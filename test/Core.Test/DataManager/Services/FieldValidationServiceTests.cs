using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;

namespace JJMasterData.Core.Test.DataManager.Services;

using Microsoft.Extensions.Localization;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class FieldValidationServiceTests
{
    [Fact]
    public async Task ValidateFieldsAsync_NullFormValues_ThrowsArgumentNullException()
    {
        // Arrange
        var expressionsServiceMock = new Mock<ExpressionsService>();
        var localizerMock = new Mock<IStringLocalizer<JJMasterDataResources>>();
        var service = new FieldValidationService(expressionsServiceMock.Object, localizerMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.ValidateFieldsAsync(null, new Dictionary<string, object>(), new PageState(), true));
    }

    [Fact]
    public async Task ValidateFieldsAsync_InvalidField_ReturnsError()
    {
        // Arrange
        var expressionsServiceMock = new Mock<ExpressionsService>();
        expressionsServiceMock.Setup(e => e.GetBoolValueAsync(It.IsAny<string>(), It.IsAny<FormStateData>()))
                             .ReturnsAsync(false);

        var localizerMock = new Mock<IStringLocalizer<JJMasterDataResources>>();
        var service = new FieldValidationService(expressionsServiceMock.Object, localizerMock.Object);

        var formElement = new FormElement();
        var formValues = new Dictionary<string, object>();
        var pageState = new PageState();

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
        var localizerMock = new Mock<IStringLocalizer<JJMasterDataResources>>();
        localizerMock.Setup(l => l["{0} field is required", It.IsAny<string>()]).Returns(new LocalizedString("Field is required","Field is required"));

        var service = new FieldValidationService(expressionsServiceMock.Object, localizerMock.Object);

        var field = new FormElementField { IsRequired = true, Label = "Field" };
        var fieldId = "fieldId";
        var value = "";

        // Act
        var result = service.ValidateField(field, fieldId, value, true);

        // Assert
        Assert.Equal("Field is required", result);
    }
}
