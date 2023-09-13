#nullable enable

using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataManager;

public class FormValuesService : IFormValuesService
{
    private IEntityRepository EntityRepository { get; }
    private IFieldValuesService FieldValuesService { get; }
    private IDataItemService DataItemService { get; }
    private ILookupService LookupService { get; }
    private IEncryptionService EncryptionService { get; }
    private IHttpContext CurrentContext { get; }
    public FormValuesService(
        IEntityRepository entityRepository,
        IFieldValuesService fieldValuesService,
        IDataItemService dataItemService,
        ILookupService lookupService,
        IEncryptionService encryptionService,
        IHttpContext currentContext)
    {
        EntityRepository = entityRepository;
        FieldValuesService = fieldValuesService;
        DataItemService = dataItemService;
        LookupService = lookupService;
        EncryptionService = encryptionService;
        CurrentContext = currentContext;
    }

    public async Task<IDictionary<string, object?>> GetFormValuesAsync(FormElement formElement, PageState pageState,
        string? fieldPrefix = null)
    {
        if (formElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var field in formElement.Fields)
        {
            var fieldName = (fieldPrefix ?? string.Empty) + field.Name;
            var value = field.ValidateRequest
                ? CurrentContext.Request.GetFormValue(fieldName)
                : CurrentContext.Request.GetUnvalidated(fieldName);


            switch (field.Component)
            {
                case FormComponent.Search:
                    {
                        var formData = new FormStateData(values, pageState);
                        value = await DataItemService.GetSelectedValueAsync(field, formData);
                        break;
                    }
                case FormComponent.Lookup:
                    {
                        value = LookupService.GetSelectedValue(fieldName);
                        break;
                    }
                case FormComponent.Slider:
                    if (double.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture,
                            out var doubleValue))
                        value = doubleValue;
                    break;
                case FormComponent.Currency:
                case FormComponent.Number:
                    string context = CurrentContext.Request.QueryString["context"];
                    if (value != null && ("panelReload".Equals(context) || "gridViewRow".Equals(context) ||
                                          "htmlContent".Equals(context)))
                    {
                        if (double.TryParse(value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture,
                                out var numericValue))
                            value = numericValue;
                        else
                            value = 0;
                    }

                    break;
                case FormComponent.CheckBox:
                    value ??= CurrentContext.Request.GetFormValue($"{fieldName}_hidden");
                    break;
            }

            if (value != null)
            {
                values.Add(field.Name, value);
            }
        }

        return values;
    }

    public async Task<IDictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        PageState pageState,
        bool autoReloadFormFields,
        string? fieldPrefix = null)
    {
        var dbValues = await GetDbValues(formElement);
        return await GetFormValuesWithMergedValuesAsync(formElement, pageState, dbValues, autoReloadFormFields, fieldPrefix);
    }

    public async Task<IDictionary<string, object?>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        PageState pageState,
        IDictionary<string, object?>? values,
        bool autoReloadFormFields,
        string? prefix = null)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        var valuesToBeReceived = new Dictionary<string, object?>();
        DataHelper.CopyIntoDictionary(valuesToBeReceived, values, true);

        if (CurrentContext.Request.IsPost && autoReloadFormFields)
        {
            var requestedValues = await GetFormValuesAsync(formElement, pageState, prefix);
            DataHelper.CopyIntoDictionary(valuesToBeReceived, requestedValues, true);
        }

        return await FieldValuesService.MergeWithExpressionValuesAsync(formElement, valuesToBeReceived, pageState, !CurrentContext.Request.IsPost);
    }



    private async Task<IDictionary<string, object?>?> GetDbValues(Element element)
    {
        string encryptedPkValues = CurrentContext.Request[
            $"data-panel-pk-values-{ComponentNameGenerator.Create(element.Name)}"];
        if (string.IsNullOrEmpty(encryptedPkValues))
            return null;

        string pkValues = EncryptionService.DecryptStringWithUrlUnescape(encryptedPkValues)!;
        var filters = DataHelper.GetPkValues(element, pkValues, '|');

        return await EntityRepository.GetFieldsAsync(element, filters);
    }
}