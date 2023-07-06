﻿#nullable enable
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Factories;

namespace JJMasterData.Core.DataManager;

public class FormValuesService : IFormValuesService
{
    private IEntityRepository EntityRepository { get; }
    private IFieldValuesService FieldValuesService { get; }
    private IDataItemService DataItemService { get; }
    private LookupFactory LookupFactory { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IHttpContext CurrentContext { get; }
    public FormValuesService(
        IEntityRepository entityRepository,
        IFieldValuesService fieldValuesService,
        IDataItemService dataItemService,
        LookupFactory lookupFactory,
        JJMasterDataEncryptionService encryptionService,
        IHttpContext currentContext)
    {
        EntityRepository = entityRepository;
        FieldValuesService = fieldValuesService;
        DataItemService = dataItemService;
        LookupFactory = lookupFactory;
        EncryptionService = encryptionService;
        CurrentContext = currentContext;
    }

    public IDictionary<string,dynamic> GetFormValues(FormElement formElement,PageState pageState, string? fieldPrefix = null)
    {
        if (formElement == null)
            throw new ArgumentException(nameof(FormElement));

        var values = new Dictionary<string,dynamic>(StringComparer.InvariantCultureIgnoreCase);
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
                    value = DataItemService.GetSelectedValue(field,null,values,pageState);
                    break;
                }
                case FormComponent.Lookup:
                {
                    //TODO: use a service instead of the component
                    var lookup = LookupFactory.CreateLookup(field, new ExpressionOptions(null, values, pageState),null,null);
                    lookup.AutoReloadFormFields = true;
                    value = lookup.SelectedValue;
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
                    if (value != null && ("reloadpainel".Equals(requestType) || "tablerow".Equals(requestType) ||
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

    public async Task<IDictionary<string, dynamic>> GetFormValuesWithMergedValues(
        FormElement formElement, 
        PageState pageState,
        bool autoReloadFormFields,
        string? fieldPrefix = null)
    {
        var dbValues = await GetDbValues(formElement);
        return await GetFormValuesWithMergedValues(formElement, pageState, dbValues, autoReloadFormFields, fieldPrefix);
    }

    public async Task<IDictionary<string,dynamic>> GetFormValuesWithMergedValues(
        FormElement formElement, 
        PageState pageState, 
        IDictionary<string,dynamic>? values,
        bool autoReloadFormFields,
        string? prefix = null)
    {
        if (formElement == null)
            throw new ArgumentNullException(nameof(formElement));

        IDictionary<string,dynamic> newValues = new Dictionary<string,dynamic>();
        DataHelper.CopyIntoDictionary(ref newValues, values, true);

        if (CurrentContext.IsPost && autoReloadFormFields)
        {
            var requestedValues = GetFormValues(formElement,pageState, prefix);
            DataHelper.CopyIntoDictionary(ref newValues, requestedValues, true);
        }
        
        return FieldValuesService.MergeWithExpressionValues(formElement,newValues, pageState, !CurrentContext.IsPost);
    }
    
    

    private async Task<IDictionary<string,dynamic>?> GetDbValues(Element element)
    {
        if (!CurrentContext.HasContext())
            return null;

        string encryptedPkValues = CurrentContext.Request["jjform_pkval_" + element.Name];
        if (string.IsNullOrEmpty(encryptedPkValues))
            return null;

        string pkValues = EncryptionService.DecryptStringWithUrlDecode(encryptedPkValues);
        var filters = DataHelper.GetPkValues(element, pkValues, '|');
        
        return await EntityRepository.GetDictionaryAsync(element, filters);
    }
}