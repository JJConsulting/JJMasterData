using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Commons.Resources;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Services;
using Microsoft.Extensions.Localization;
using Moq;

namespace JJMasterData.Core.Test.DataDictionary.Services;

public class DataDictionaryLocalizationServiceTests
{
    [Fact]
    public async Task GetFormElementLocalizationKeysAsync_ReturnsExpectedKeys()
    {
        var formElement = new FormElement
        {
            Name = "customer",
            TableName = "TB_CUSTOMER",
            Title = "Customer Title",
            SubTitle = "Customer SubTitle"
        };

        formElement.Panels.Add(new FormElementPanel
        {
            Title = "Panel Title",
            SubTitle = "Panel SubTitle"
        });

        formElement.Relationships.Add(new FormElementRelationship
        {
            Panel = new FormElementPanel
            {
                Title = "Relationship Title",
                SubTitle = "Relationship SubTitle"
            }
        });

        var field = new FormElementField
        {
            Name = "customerName",
            Label = "Customer Name",
            HelpDescription = "Customer Name Tooltip"
        };

        field.Actions.Add(new UrlRedirectAction
        {
            Name = "open-customer-details",
            Text = "Open details",
            Tooltip = "Open details tooltip",
            ModalTitle = "Open details title"
        });

        formElement.Fields.Add(field);

        formElement.Options.GridToolbarActions.Clear();
        formElement.Options.GridToolbarActions.Add(new InsertAction
        {
            Text = "Insert Text",
            Tooltip = "Insert Tooltip",
            ModalTitle = "Insert Modal Title"
        });

        formElement.Options.FormToolbarActions.Clear();
        formElement.Options.FormToolbarActions.Add(new SaveAction
        {
            Text = "Save Text",
            Tooltip = "Save Tooltip"
        });

        formElement.Options.GridTableActions.Clear();
        formElement.Options.GridTableActions.Add(new EditAction
        {
            Tooltip = "Edit Tooltip",
            ModalTitle = "Edit Modal Title"
        });

        var repositoryMock = new Mock<IDataDictionaryRepository>();
        repositoryMock
            .Setup(x => x.GetFormElementListAsync(It.IsAny<bool?>()))
            .ReturnsAsync([formElement]);

        var localizerMock = BuildLocalizerMock();
        var service = new DataDictionaryLocalizationService(repositoryMock.Object, localizerMock.Object);

        var keys = await service.GetFormElementLocalizationKeysAsync();

        Assert.Contains("customerName", keys);
        Assert.Contains("Customer Name", keys);
        Assert.Contains("Customer Name Tooltip", keys);
        Assert.Contains("open-customer-details", keys);
        Assert.Contains("Open details", keys);
        Assert.Contains("Open details tooltip", keys);
        Assert.Contains("Open details title", keys);
        Assert.Contains("Insert Modal Title", keys);
        Assert.Contains("Edit Modal Title", keys);
        Assert.Contains("Customer Title", keys);
        Assert.Contains("Customer SubTitle", keys);
        Assert.Contains("Panel Title", keys);
        Assert.Contains("Panel SubTitle", keys);
        Assert.Contains("Relationship Title", keys);
        Assert.Contains("Relationship SubTitle", keys);
    }

    [Fact]
    public void GetCommonsResourceKeys_ReturnsResourceKeys()
    {
        var keys = DataDictionaryLocalizationService.GetCommonsResourceKeys();

        Assert.Contains("Add", keys);
        Assert.Contains("Actions", keys);
    }

    [Fact]
    public async Task GetAllLocalizationKeysAsync_ReturnsUnionFromFormElementsAndResources()
    {
        var formElement = new FormElement
        {
            Name = "test",
            TableName = "TB_TEST",
            Title = "UniqueFormElementTitleKey"
        };

        var repositoryMock = new Mock<IDataDictionaryRepository>();
        var localizerMock = BuildLocalizerMock();
        repositoryMock
            .Setup(x => x.GetFormElementListAsync(It.IsAny<bool?>()))
            .ReturnsAsync(new List<FormElement> { formElement });

        var service = new DataDictionaryLocalizationService(repositoryMock.Object, localizerMock.Object);

        var keys = await service.GetAllLocalizationKeysAsync();

        Assert.Contains("UniqueFormElementTitleKey", keys);
        Assert.Contains("Add", keys);
    }

    [Fact]
    public async Task GetLocalizationDictionaryAsync_TranslatesKeysUsingRequestedCulture()
    {
        var formElement = new FormElement
        {
            Name = "test",
            TableName = "TB_TEST",
            Title = "UniqueFormElementTitleKey"
        };

        var repositoryMock = new Mock<IDataDictionaryRepository>();
        repositoryMock
            .Setup(x => x.GetFormElementListAsync(It.IsAny<bool?>()))
            .ReturnsAsync(new List<FormElement> { formElement });

        var localizerMock = BuildLocalizerMock();
        var service = new DataDictionaryLocalizationService(repositoryMock.Object, localizerMock.Object);

        var dictionary = await service.GetLocalizationDictionaryAsync(new CultureInfo("pt-BR"));

        Assert.Equal("pt-BR:UniqueFormElementTitleKey", dictionary["UniqueFormElementTitleKey"]);
        Assert.Equal("pt-BR:Add", dictionary["Add"]);
    }

    private static Mock<IStringLocalizer<MasterDataResources>> BuildLocalizerMock()
    {
        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        localizerMock
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string key) =>
                new LocalizedString(key, $"{CultureInfo.CurrentUICulture.Name}:{key}", resourceNotFound: false));

        return localizerMock;
    }
}
