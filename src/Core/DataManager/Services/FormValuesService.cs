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

namespace JJMasterData.Core.DataManager;

public class FormValuesService : IFormValuesService
{
    private IEntityRepository EntityRepository { get; }
    private IFieldValuesService FieldValuesService { get; }
    private IDataItemService DataItemService { get; }
    private ILookupService LookupService { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IHttpContext CurrentContext { get; }
    public FormValuesService(
        IEntityRepository entityRepository,
        IFieldValuesService fieldValuesService,
        IDataItemService dataItemService,
        ILookupService lookupService,
        JJMasterDataEncryptionService encryptionService,
        IHttpContext currentContext)
    {
        EntityRepository = entityRepository;
        FieldValuesService = fieldValuesService;
        DataItemService = dataItemService;
        LookupService = lookupService;
        EncryptionService = encryptionService;
        CurrentContext = currentContext;
    }

    public async Task<IDictionary<string, dynamic>> GetFormValuesAsync(FormElement formElement, PageState pageState,
        string? fieldPrefix = null)
    {
        if (formElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Dictionary<string, dynamic>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var field in formElement.Fields)
        {
            var fieldName = (fieldPrefix ?? string.Empty) + field.Name;
            var value = field.ValidateRequest
                ? CurrentContext.Request.Form(fieldName)
                : CurrentContext.Request.GetUnvalidated(fieldName);


            switch (field.Component)
            {
                case FormComponent.Search:
                    {
                        value = await DataItemService.GetSelectedValueAsync(field, null, values, pageState);
                        break;
                    }
                case FormComponent.Lookup:
                    {
                        value = LookupService.GetSelectedValue(field.Name);
                        break;
                    }
                case FormComponent.Slider:
                    if (double.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture,
                            out var doubleValue))
                        value = doubleValue;
                    break;
                case FormComponent.Currency:
                case FormComponent.Number:
                    string requestType = CurrentContext.Request.QueryString("t");
                    if (value != null && ("reloadPanel".Equals(requestType) || "tablerow".Equals(requestType) ||
                                          "ajax".Equals(requestType)))
                    {
                        if (double.TryParse(value?.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture,
                                out var numericValue))
                            value = numericValue;
                        else
                            value = 0;
                    }

                    break;
                case FormComponent.CheckBox:
                    value ??= CurrentContext.Request.Form(fieldName + "_hidden");
                    break;
            }

            if (value != null)
            {
                values.Add(field.Name, value);
            }
        }

        return values;
    }

    public async Task<IDictionary<string, dynamic>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        PageState pageState,
        bool autoReloadFormFields,
        string? fieldPrefix = null)
    {
        var dbValues = await GetDbValues(formElement);
        return await GetFormValuesWithMergedValuesAsync(formElement, pageState, dbValues, autoReloadFormFields, fieldPrefix);
    }

    public async Task<IDictionary<string, dynamic>> GetFormValuesWithMergedValuesAsync(
        FormElement formElement,
        PageState pageState,
        IDictionary<string, dynamic>? values,
        bool autoReloadFormFields,
        string? prefix = null)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        var valuesToBeReceived = new Dictionary<string, dynamic>();
        DataHelper.CopyIntoDictionary(valuesToBeReceived, values, true);

        if (CurrentContext.IsPost && autoReloadFormFields)
        {
            var requestedValues = await GetFormValuesAsync(formElement, pageState, prefix);
            DataHelper.CopyIntoDictionary(valuesToBeReceived, requestedValues, true);
        }

        return await FieldValuesService.MergeWithExpressionValuesAsync(formElement, valuesToBeReceived, pageState, !CurrentContext.IsPost);
    }



    private async Task<IDictionary<string, dynamic>?> GetDbValues(Element element)
    {
        if (!CurrentContext.HasContext())
            return null;

        string encryptedPkValues = CurrentContext.Request["jjform_pkval_" + element.Name];
        if (string.IsNullOrEmpty(encryptedPkValues))
            return null;

        string pkValues = EncryptionService.DecryptStringWithUrlUnescape(encryptedPkValues);
        var filters = DataHelper.GetPkValues(element, pkValues, '|');

        return await EntityRepository.GetDictionaryAsync(element, filters);
    }
}