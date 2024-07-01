using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class DataPanelFactory(IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IHttpContext httpContext,
        IEncryptionService encryptionService,
        FieldFormattingService fieldFormattingService,
        FieldValidationService fieldValidationService,
        FormValuesService formValuesService,
        ExpressionsService expressionsService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        IComponentFactory componentFactory,
        UrlRedirectService urlRedirectService)
    : IFormElementComponentFactory<JJDataPanel>
{
    public JJDataPanel Create(FormElement formElement)
    {
        return new JJDataPanel(
            formElement, 
            entityRepository, 
            httpContext,
            encryptionService, 
            fieldFormattingService, 
            fieldValidationService,
            formValuesService, 
            expressionsService,
            urlRedirectService,
            stringLocalizer,
            componentFactory);
    }

    public async Task<JJDataPanel> CreateAsync(string elementName)
    {
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        return Create(formElement);
    }
}